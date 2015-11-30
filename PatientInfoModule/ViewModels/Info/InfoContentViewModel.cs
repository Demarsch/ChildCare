using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace PatientInfoModule.ViewModels
{
    public class InfoContentViewModel : TrackableBindableBase, INavigationAware, IActiveDataErrorInfo, IChangeTrackerMediator, IDisposable
    {
        private readonly IPatientService patientService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog log;

        private readonly PatientInfoViewModel patientInfo;

        private readonly Func<PatientInfoViewModel> relativeInfoFactory;

        public InfoContentViewModel(IPatientService patientService,
                                    IEventAggregator eventAggregator,
                                    ILog log,
                                    PatientInfoViewModel patientInfo,
                                    Func<PatientInfoViewModel> relativeInfoFactory)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (patientInfo == null)
            {
                throw new ArgumentNullException("patientInfo");
            }
            if (relativeInfoFactory == null)
            {
                throw new ArgumentNullException("relativeInfoFactory");
            }
            this.patientService = patientService;
            this.eventAggregator = eventAggregator;
            this.log = log;
            this.patientInfo = patientInfo;
            this.relativeInfoFactory = relativeInfoFactory;
            currentPatientId = SpecialValues.NonExistingId;
            Relatives = new ObservableCollectionEx<PatientInfoViewModel>();
            Relatives.BeforeCollectionChanged += RelativesOnBeforeCollectionChanged;
            changeTracker = new CompositeChangeTracker(patientInfo.ChangeTracker, new ObservableCollectionChangeTracker<PatientInfoViewModel>(Relatives));
            changeTracker.PropertyChanged += OnChangesTracked;
            selectedPatientOrRelative = patientInfo;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            createNewPatientCommand = new DelegateCommand(CreateNewPatient);
            saveChangesCommand = new DelegateCommand(SaveChangesAsync, CanSaveChanges);
            cancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
            addRelativeCommand = new DelegateCommand(AddRelative, CanAddRelative);
            saveChangesCommandWrapper = new CommandWrapper { Command = SaveChangesCommand };
            loadRelativeListWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await LoadRelativeListAsync(currentPatientId)) };
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                UpdateCommandsState();
            }
        }

        private void UpdateCommandsState()
        {
            saveChangesCommand.RaiseCanExecuteChanged();
            cancelChangesCommand.RaiseCanExecuteChanged();
            addRelativeCommand.RaiseCanExecuteChanged();
        }

        #region Actions

        private readonly DelegateCommand createNewPatientCommand;

        private readonly DelegateCommand saveChangesCommand;

        private readonly DelegateCommand cancelChangesCommand;

        private readonly DelegateCommand addRelativeCommand;

        private readonly CommandWrapper saveChangesCommandWrapper;

        private readonly CommandWrapper loadRelativeListWrapper;

        public ICommand CreateNewPatientCommand
        {
            get { return createNewPatientCommand; }
        }

        private void CreateNewPatient()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Publish(SpecialValues.NewId);
        }

        public ICommand SaveChangesCommand
        {
            get { return saveChangesCommand; }
        }

        private CancellationTokenSource currentOperationToken;

        private async void SaveChangesAsync()
        {
            FailureMediator.Deactivate();
            if (!Validate())
            {
                return;
            }
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            log.InfoFormat("Saving relatives data for patient with Id = {0}", currentPatientId == SpecialValues.NewId ? "(New patient)" : currentPatientId.ToString());
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccessfull = false;
            var isNewPatient = patientInfo.CurrentPerson.Id.IsNewOrNonExisting();
            PatientInfoViewModel personToSave = null;
            try
            {
                personToSave = patientInfo;
                await patientInfo.SaveChangesAsync(token);
                if (isNewPatient)
                {
                    currentPatientId = patientInfo.CurrentPerson.Id;
                    eventAggregator.GetEvent<SelectionEvent<Person>>().Publish(currentPatientId);
                }
                foreach (var relative in Relatives.Where(x => x.CurrentPerson.Id.IsNewOrNonExisting() || x.ChangeTracker.HasChanges))
                {
                    personToSave = relative;
                    await relative.SaveChangesAsync(token, patientInfo.CurrentPerson);
                }
                personToSave = null;
                saveSuccessfull = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                //At this point patint himself is already saved
                log.ErrorFormatEx(ex, "Failed to save relative relationship for patient with Id {0}", patientInfo.CurrentPerson.Id);
                FailureMediator.Activate("Не удалось сохранить родственников пациента. Попробуйте еще раз. Если ошибка повторится, пожалуйста, обратитесь в службу поддержки",
                                         saveChangesCommandWrapper,
                                         ex,
                                         true);
                SelectedPatientOrRelative = personToSave;
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccessfull)
                {
                    CancelValidation();
                    ChangeTracker.AcceptChanges();
                    ChangeTracker.IsEnabled = true;
                    UpdateCommandsState();
                }
            }
        }

        private bool CanSaveChanges()
        {
            if (currentPatientId == SpecialValues.NewId)
            {
                return true;
            }
            return ChangeTracker.HasChanges;
        }

        public ICommand CancelChangesCommand
        {
            get { return cancelChangesCommand; }
        }

        private void CancelChanges()
        {
            FailureMediator.Deactivate();
            ChangeTracker.RestoreChanges();
            CancelValidation();
        }

        private bool CanCancelChanges()
        {
            if (currentPatientId == SpecialValues.NewId)
            {
                return false;
            }
            return ChangeTracker.HasChanges;
        }

        public ICommand AddRelativeCommand
        {
            get { return addRelativeCommand; }
        }

        private void AddRelative()
        {
            var newRelative = relativeInfoFactory();
            newRelative.IsRelative = true;
            Relatives.Add(newRelative);
            SelectedPatientOrRelative = newRelative;
        }

        private bool CanAddRelative()
        {
            return currentPatientId != SpecialValues.NonExistingId;
        }

        #endregion

        private void RelativesOnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<PatientInfoViewModel>())
                {
                    changeTracker.RemoveTracker(oldItem.ChangeTracker);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<PatientInfoViewModel>())
                {
                    changeTracker.AddTracker(newItem.ChangeTracker);
                }
            }
        }

        public ObservableCollectionEx<PatientInfoViewModel> Relatives { get; private set; }

        private PatientInfoViewModel selectedPatientOrRelative;

        public PatientInfoViewModel SelectedPatientOrRelative
        {
            get { return selectedPatientOrRelative; }
            set { SetProperty(ref selectedPatientOrRelative, value); }
        }


        private int currentPatientId;

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            SelectedPatientOrRelative = patientInfo;
            await Task.WhenAll(LoadRelativeListAsync(targetPatientId), patientInfo.LoadPatientInfoAsync(targetPatientId, CancellationToken.None));
        }

        private async Task LoadRelativeListAsync(int patientId)
        {
            if (Interlocked.Exchange(ref currentPatientId, patientId) == patientId)
            {
                return;
            }
            ClearData();
            UpdateCommandsState();
            if (patientId == SpecialValues.NewId || patientId == SpecialValues.NonExistingId)
            {
                return;
            }
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка данных пациента...");
            log.InfoFormat("Loading patient info for patient with Id {0}...", patientId);
            try
            {
                var relatives = await patientService.GetRelativesAsync(patientId);
                foreach (var relative in relatives)
                {
                    var viewModel = relativeInfoFactory();
                    viewModel.SelectedRelationship = viewModel.Relationships.FirstOrDefault(x => x.Id == relative.RelativeRelationshipId);
                    //We don't need to wait while relative data is loaded
                    viewModel.LoadPatientInfoAsync(relative.RelativeId, token);
                    Relatives.Add(viewModel);
                }
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load common patient info for patient with Id {0}", patientId);
                FailureMediator.Activate("Не удалость загрузить данные пациента. Попробуйте еще раз или обратитесь в службу поддержки", loadRelativeListWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    ChangeTracker.IsEnabled = true;
                    UpdateCommandsState();
                    BusyMediator.Deactivate();
                }
            }
        }

        private void ClearData()
        {
            Relatives.Clear();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public bool Validate()
        {
            PatientInfoViewModel invalidPerson = null;
            var result = patientInfo.Validate();
            if (!result)
            {
                invalidPerson = patientInfo;
            }
            foreach (var relative in Relatives)
            {
                result |= relative.Validate();
                if (!result && invalidPerson == null)
                {
                    invalidPerson = relative;
                }
            }
            if (invalidPerson != null)
            {
                SelectedPatientOrRelative = invalidPerson;
            }
            return result;
        }

        public void CancelValidation()
        {
            patientInfo.CancelValidation();
            foreach (var relative in Relatives)
            {
                relative.CancelValidation();
            }
        }

        private readonly CompositeChangeTracker changeTracker;

        public IChangeTracker ChangeTracker { get { return changeTracker; } }

        public void Dispose()
        {
            ChangeTracker.Dispose();
            Relatives.BeforeCollectionChanged -= RelativesOnBeforeCollectionChanged;
        }
    }
}