using System.Globalization;
using System.Threading;
using System.Windows;
using Core;
using DataLib;
using log4net;
using log4net.Core;
using TestCore;
using System;
using Environment = Core.Environment;

namespace Registry
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var newCulture = new CultureInfo("ru-RU", true);
            newCulture.DateTimeFormat.ShortDatePattern = "dd MMM yyyy";
            Thread.CurrentThread.CurrentCulture = newCulture;
            base.OnStartup(e);
            var log = new LogImpl(LoggerManager.CreateRepository(typeof(App).FullName).GetLogger(typeof(App).Name)) as ILog;
            var contextProvider = new ModelContextProvider() as IDataContextProvider;
            var dataAccessLayer = new ModelAccessLayer(contextProvider);
            var serviceLocator = new MainServiceLocator();
            serviceLocator.Add(typeof(ILog), log);
            serviceLocator.Add(typeof(IUserSystemInfoService), new ADUserSystemInfoService());
            serviceLocator.Add(typeof(IDataAccessLayer), dataAccessLayer);
            var personService = new PersonService(serviceLocator);
            serviceLocator.Add(typeof(IPersonService), personService);
            serviceLocator.Add(typeof(IPermissionService), new PermissionService(serviceLocator));
            serviceLocator.Add(typeof(IUserService), new UserService(serviceLocator));

            var cacheService = new DataContextCacheService(contextProvider) as ICacheService;
            var environment = new Environment(contextProvider) as IEnvironment;
            var dialogService = new WindowDialogService() as IDialogService;

            var patientAssignmentService = new PatientAssignmentService(contextProvider) as IPatientAssignmentService;
            var patientAssignmentListViewModel = new PatientAssignmentListViewModel(patientAssignmentService, log, cacheService);

            var scheduleService = new ScheduleService(contextProvider, environment);
            var scheduleViewModel = new ScheduleViewModel(scheduleService, log, cacheService, environment, dialogService);

            var patientService = new PatientService(contextProvider) as IPatientService;
            var patientSearchViewModel = new PatientSearchViewModel(patientService, personService, log, dialogService, patientAssignmentListViewModel);
            var mainViewModel = new MainWindowViewModel(patientSearchViewModel, scheduleViewModel);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
