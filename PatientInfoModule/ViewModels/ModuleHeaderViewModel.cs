using System;
using System.Windows;
using Core.Data;
using Core.Data.Services;
using Core.Wpf.Events;
using log4net;
using Prism;
using Prism.Events;
using Prism.Mvvm;

namespace PatientInfoModule.ViewModels
{
    public class ModuleHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ILog log;

        private readonly IEventAggregator eventAggregator;

        private const string PatientIsNotSelected = "не выбран";

        public ModuleHeaderViewModel(IDbContextProvider contextProvider, ILog log, IEventAggregator eventAggregator)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.contextProvider = contextProvider;
            this.log = log;
            this.eventAggregator = eventAggregator;
            ShortName = PatientIsNotSelected;
            SubscribeToEvents();
        }

        private string shortName;

        public string ShortName
        {
            get { return shortName; }
            set { SetProperty(ref shortName, value); }
        }

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void OnPatientSelected(int patientId)
        {
            MessageBox.Show("Patient is selected and his Id is " + patientId + " and his data should be loaded into 1st ribbon tab");
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    IsActiveChanged(this, EventArgs.Empty);
                    OnPropertyChanged(() => IsActive);
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
