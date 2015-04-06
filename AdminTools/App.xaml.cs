using System.Globalization;
using System.Threading;
using System.Windows;
using Core;
using DataLib;
using AdminTools.ViewModel;

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
            var contextProvider = new ModelContextProvider();

            var mainViewModel = new MainWindowViewModel();
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
