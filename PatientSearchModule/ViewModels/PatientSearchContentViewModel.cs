using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Services;
using Core.Wpf.ViewModels;
using Prism.Events;
using Prism.Mvvm;
using Shared.Patient.ViewModels;

namespace PatientSearchModule.ViewModels
{
    public class PatientSearchContentViewModel : BindableBase, IDisposable
    {
        private readonly IEventAggregator eventAggregator;

        private readonly IDialogServiceAsync dialogService;

        public PatientSearchContentViewModel(IEventAggregator eventAggregator, IDialogServiceAsync dialogService, PersonSearchViewModel personSearchViewModel)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (personSearchViewModel == null)
            {
                throw new ArgumentNullException("personSearchViewModel");
            }
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            Header = "Поиск Пациента";
            PersonSearchViewModel = personSearchViewModel;
            personSearchViewModel.PropertyChanged += OnSelectedPatientChangedAsync;
        }

        private async void OnSelectedPatientChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || string.Equals(e.PropertyName, "SelectedPersonId", StringComparison.Ordinal))
            {
                await SelectPatientAsync(PersonSearchViewModel.SelectedPersonId);
            }
        }

        private object header;

        public object Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        public PersonSearchViewModel PersonSearchViewModel { get; private set; }

        private async Task SelectPatientAsync(int patientId)
        {
            if (patientId.IsNewOrNonExisting())
            {
                return;
            }
            var eventData = new BeforeSelectionChangedEventData(patientId);
            eventAggregator.GetEvent<BeforeSelectionChangedEvent<Person>>().Publish(eventData);
            if (eventData.IsCancelled)
            {
                PersonSearchViewModel.SelectedPersonId = SpecialValues.NonExistingId;
                return;
            }
            if (eventData.ActionsToPerform.Any())
            {
                var confirmation = new ConfirmationDialogViewModel
                                   {
                                       CancelButtonText = "Отменить изменения",
                                       ConfirmButtonText = "Сохранить изменения",
                                       Question = "Данные текущего пациента изменились. Сохранить эти изменения?",
                                       Title = "Подтверждение"
                                   };
                var result = await dialogService.ShowDialogAsync(confirmation);
                if (result == null)
                {
                    PersonSearchViewModel.SelectedPersonId = SpecialValues.NonExistingId;
                    return;
                }
                if (result == true)
                {
                    var runningActions = eventData.ActionsToPerform.Select(x => new { ActionResult = x.Action(), x.OnFail }).ToArray();
                    await Task.WhenAll(runningActions.Select(x => x.ActionResult));
                    var firstFailedAction = runningActions.FirstOrDefault(x => !x.ActionResult.Result);
                    if (firstFailedAction != null)
                    {
                        firstFailedAction.OnFail();
                        PersonSearchViewModel.SelectedPersonId = SpecialValues.NonExistingId;
                        return;
                    }
                }
            }
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Publish(patientId);
        }

        public void Dispose()
        {
            PersonSearchViewModel.PropertyChanged -= OnSelectedPatientChangedAsync;
        }
    }
}