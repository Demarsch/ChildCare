using System;
using System.ComponentModel;
using System.Windows.Navigation;
using Core.Data.Misc;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;

namespace Shared.Patient.ViewModels
{
    public class PersonSearchDialogViewModel : BindableBase, IDialogViewModel, IDisposable
    {
        public PersonSearchDialogViewModel(PersonSearchViewModel personSearchViewModel)
        {
            if (personSearchViewModel == null)
            {
                throw new ArgumentNullException("personSearchViewModel");
            }
            PersonSearchViewModel = personSearchViewModel;
            personSearchViewModel.PropertyChanged += OnSelectedPatientChanged;
            CloseCommand = new DelegateCommand<bool?>(Close);
        }

        private void OnSelectedPatientChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || string.Equals(e.PropertyName, "SelectedPersonId"))
            {
                if (PersonSearchViewModel.SelectedPersonId.IsNewOrNonExisting())
                {
                    return;
                }
                OnCloseRequested(true);
            }
        }

        public PersonSearchViewModel PersonSearchViewModel { get; private set; }

        private void Close(bool? confirm)
        {
            OnCloseRequested(confirm ?? false);
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public string ConfirmButtonText { get { return string.Empty; } }

        public string CancelButtonText { get { return "Отмена"; } }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(bool patientIsSelected)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, new ReturnEventArgs<bool>(patientIsSelected));
            }
        }

        public void Dispose()
        {
            PersonSearchViewModel.PropertyChanged -= OnSelectedPatientChanged;
        }
    }
}
