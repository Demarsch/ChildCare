using System.Globalization;
using System.Threading;
using System.Windows;
using Core;
using DataLib;
using log4net;
using log4net.Core;

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
            var log = new LogImpl(LoggerManager.CreateRepository(typeof (App).FullName).GetLogger(typeof (App).Name)) as ILog;
            var contextProvider = new ModelContextProvider() as IDataContextProvider;
            var cacheService = new DataContextCacheService(contextProvider) as ICacheService;

            var patientAssignmentService = new PatientAssignmentService(contextProvider) as IPatientAssignmentService;
            var patientAssignmentListViewModel = new PatientAssignmentListViewModel(patientAssignmentService, log, cacheService);

            var scheduleViewModel = new ScheduleViewModel();

            var patientService = new PatientService(contextProvider) as IPatientService;
            var patientSearchViewModel = new PatientSearchViewModel(patientService, log, patientAssignmentListViewModel);
            var mainViewModel = new MainWindowViewModel(patientSearchViewModel, scheduleViewModel);
            var mainWindow = new MainWindow {DataContext = mainViewModel};
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
