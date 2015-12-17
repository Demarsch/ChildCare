using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Services;
using Core.Misc;
using Core.Wpf.Events;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Events;
using Prism.Mvvm;

namespace Shell
{
    public class ShellWindowViewModel : BindableBase, IDisposable
    {
        private readonly IDbContextProvider contextProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly IEnvironment environment;

        private readonly ILog log;

        private readonly IList<IDisposable> disposables;

        public ShellWindowViewModel(IDbContextProvider contextProvider, IEventAggregator eventAggregator, IEnvironment environment, ILog log)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.contextProvider = contextProvider;
            this.eventAggregator = eventAggregator;
            this.environment = environment;
            this.log = log;
            disposables = new List<IDisposable>();
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            SubscribeToEvents();
        }

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        public bool CanOpenMenu { get { return !FailureMediator.IsActive && !BusyMediator.IsActive; } }

        private bool isMenuOpen;

        public bool IsMenuOpen
        {
            get { return isMenuOpen; }
            set { SetProperty(ref isMenuOpen, value); }
        }

        public async Task<bool> CheckDatabaseConnectionAsync()
        {
            BusyMediator.Activate("Подключение к базе данных...");
            OnPropertyChanged(() => CanOpenMenu);
            try
            {
                await Task.Run((Action)CheckDatabaseConnection);
                await Task.Run((Action)CheckCurrentUserIsAccessible);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to validate database connection", ex);
                FailureMediator.Activate("В процессе подключения к базе данных возникла ошибка. Пожалуйста, обратитесь в службу поддержки", exception: ex);
                return false;
            }
            finally
            {
                BusyMediator.Deactivate();
                OnPropertyChanged(() => CanOpenMenu);
            }
        }

        private void CheckDatabaseConnection()
        {
            using (var context = contextProvider.CreateNewContext())
            {
                if (context.Database.Exists())
                {
                    return;
                }
                throw new ApplicationException("База данных не существует");
            }
        }

        private void CheckCurrentUserIsAccessible()
        {
            var serverDate = environment.CurrentDate.ToString(DateTimeFormats.ShortDateTimeFormat);
            var currentUserSid = environment.CurrentUser.SID;
            log.InfoFormat("Application is run under user with SID '{0}', current server time is {1}", currentUserSid, serverDate);
        }

        private void SubscribeToEvents()
        {
            disposables.Add(eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Subscribe(OnPatientSelected));
            disposables.Add(eventAggregator.GetEvent<MainMenuCloseRequestedEvent>().Subscribe(OnMainMenuCloseRequested));
        }

        private void OnMainMenuCloseRequested(object obj)
        {
            IsMenuOpen = false;
        }

        private void OnPatientSelected(int patientId)
        {
            IsMenuOpen = false;
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
