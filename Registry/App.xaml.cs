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
            var contextProvider = new ModelContextProvider();
            var patientService = new PatientService(contextProvider) as IPatientService;
            var patientSearchViewModel = new PatientSearchViewModel(patientService, log);
            var mainViewModel = new MainWindowViewModel(patientSearchViewModel);
            var mainWindow = new MainWindow {DataContext = mainViewModel};
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
