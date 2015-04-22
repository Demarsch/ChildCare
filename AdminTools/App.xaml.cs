using System.Globalization;
using System.Threading;
using System.Windows;
using Core;
using DataLib;
using AdminTools.ViewModel;
using log4net;
using log4net.Core;

namespace AdminTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainService = new MainServiceLocator();

            var mainViewModel = new MainWindowViewModel(mainService);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
