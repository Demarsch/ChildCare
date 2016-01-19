using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;
using Core.Notification;
using Core.Wpf.Events;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace Shell
{
    public class ShellWindowViewModel : BindableBase, IDisposable
    {
        private readonly IDbContextProvider contextProvider;

        private readonly IRegionManager regionManager;

        private readonly INotificationService notificationService;

        private readonly IEventAggregator eventAggregator;

        private readonly IEnvironment environment;

        private readonly ILog log;

        private readonly IList<IDisposable> disposables;

        public ShellWindowViewModel(IDbContextProvider contextProvider,
                                    IRegionManager regionManager,
                                    INotificationService notificationService,
                                    IEventAggregator eventAggregator,
                                    IEnvironment environment,
                                    ILog log)
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
            if (notificationService == null)
            {
                throw new ArgumentNullException("notificationService");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            this.contextProvider = contextProvider;
            this.regionManager = regionManager;
            this.notificationService = notificationService;
            this.eventAggregator = eventAggregator;
            this.environment = environment;
            this.log = log;
            disposables = new List<IDisposable>();
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            SubscribeToEvents();
        }

        public void HideCentralRegionContent()
        {
            var activeModuleContent = regionManager.Regions[RegionNames.ModuleContent].ActiveViews.FirstOrDefault();
            if (activeModuleContent != null)
            {
                regionManager.Regions[RegionNames.ModuleContent].Deactivate(activeModuleContent);
            }
        }

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        public bool CanOpenMenu
        {
            get { return !FailureMediator.IsActive && !BusyMediator.IsActive; }
        }

        private bool isMenuOpen;

        public bool IsMenuOpen
        {
            get { return isMenuOpen; }
            set { SetProperty(ref isMenuOpen, value); }
        }

        public async Task<bool> CheckServicesAreOnlineAsync()
        {
            BusyMediator.Activate("Подключение к базе данных...");
            OnPropertyChanged(() => CanOpenMenu);
            try
            {
                await Task.Run((Action)CheckDatabaseConnection);
                await Task.Run((Action)CheckCurrentUserIsAccessible);
                await notificationService.CheckServiceExistsAsync();
                return true;
            }
            catch (UserActivationException ex)
            {
                log.InfoFormat("Failed to log in as inactive user with SID {0}", environment.CurrentUser.SID);
                FailureMediator.Activate("Текущий пользователь неактивен поэтому не может использовать приложение");
                return false;
            }
            catch (DataNotFoundException ex)
            {
                log.Error("Failed to check notification service. " + ex.Message, ex);
                FailureMediator.Activate("В базе данных не содержится информация об адресе сервиса оповещений. Пожалуйста, обратитесь в службу поддержки", exception: ex);
                return false;
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
            var currentUser = environment.CurrentUser;
            if (currentUser.BeginDateTime.Date > DateTime.Today || currentUser.EndDateTime.Date < DateTime.Today)
            {
                throw new UserActivationException();
            }
            log.InfoFormat("Application is run under user with login '{0}', current server time is {1}", currentUser.Login, serverDate);
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