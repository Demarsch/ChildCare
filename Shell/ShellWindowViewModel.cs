using System;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Services;
using Core.Misc;
using Core.Wpf.Events;
using Core.Wpf.Mvvm;
using Prism.Events;
using Prism.Mvvm;

namespace Shell
{
    public class ShellWindowViewModel : BindableBase, IDisposable
    {
        private readonly IDbContextProvider contextProvider;

        private readonly IEventAggregator eventAggregator;

        public ShellWindowViewModel(IDbContextProvider contextProvider, IEventAggregator eventAggregator)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.contextProvider = contextProvider;
            this.eventAggregator = eventAggregator;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            SubscribeToEvents();
        }

        public BusyMediator BusyMediator { get; private set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        public bool CanOpenMenu { get { return !CriticalFailureMediator.IsActive && !BusyMediator.IsActive; } }

        private bool isMenuOpen;

        public bool IsMenuOpen
        {
            get { return isMenuOpen; }
            set { SetProperty(ref isMenuOpen, value); }
        }

        public async Task CheckDatabaseConnectionAsync()
        {
            BusyMediator.Activate("Подключение к базе данных...");
            OnPropertyChanged(() => CanOpenMenu);
            var success = await Task.Run(() => CheckDatabaseConnection());
            BusyMediator.Deactivate();
            if (!success)
            {
                CriticalFailureMediator.Activate("Не удалось подключиться к базе данных. Пожалуйста, обратитесь в службу поддержки");
            }
            OnPropertyChanged(() => CanOpenMenu);
        }

        private bool CheckDatabaseConnection()
        {
            using (var context = contextProvider.CreateNewContext())
            {
                return context.Database.Exists();
            }
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void OnPatientSelected(int patientId)
        {
            IsMenuOpen = false;
        }

        private void UnsubscribeFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}
