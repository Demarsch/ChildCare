using System.Globalization;
using System.Threading;
using System.Windows;
using Core;
using DataLib;



namespace AdminTools
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
            var contextProvider = new ModelContextProvider();

            var permissionService = new PermissionService(contextProvider) as IPermissionService;

            var permissionTreeViewModel = new PermissionTreeViewModel(permissionService);
            var mainViewModel = new MainWindowViewModel(permissionTreeViewModel);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
