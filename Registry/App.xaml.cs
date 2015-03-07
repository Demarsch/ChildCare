using System.Windows;
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
            base.OnStartup(e);
            var loggerRepository = LoggerManager.CreateRepository(typeof (App).FullName);
            var mainViewModel = new MainWindowViewModel(new LogImpl(loggerRepository.GetLogger(typeof (App).Name)), new MainService());
            var mainWindow = new MainWindow {DataContext = mainViewModel};
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
