using Core;
using DataLib;
using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Commission
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var newCulture = new CultureInfo("ru-RU", true);
            newCulture.DateTimeFormat.ShortDatePattern = "dd MMM yyyy";
            Thread.CurrentThread.CurrentCulture = newCulture;
            base.OnStartup(e);
           
            var Container = MainContainerProvider.GetContainer();

            Container.RegisterSingle<IDataContextProvider, ModelContextProvider>();
            Container.RegisterSingle<IUserSystemInfoService, ADUserSystemInfoService>();
            Container.RegisterSingle<IUserService, UserService>();
            Container.RegisterSingle<ILog>(new LogImpl(LoggerManager.CreateRepository(typeof(App).FullName).GetLogger(typeof(App).Name)));

            Container.Register<CommissionManagementViewModel>();
            Container.Register<CommissionNavigatorViewModel>();
            Container.Register<CommissionPersonFlowViewModel>();
            Container.Register<CommissionItemViewModel>();

            MainWindow = new CommissionManagementView() 
            { 
                DataContext = Container.GetInstance<CommissionManagementViewModel>() 
            };
            MainWindow.Show();
        }
    }
}
