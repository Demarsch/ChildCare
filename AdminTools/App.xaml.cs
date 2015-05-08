﻿using System.Globalization;
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

            var Container = MainContainerProvider.GetContainer();

            Container.RegisterSingle<IDataContextProvider, ModelContextProvider>();
            Container.RegisterSingle<IUserSystemInfoService, ADUserSystemInfoService>();
            Container.RegisterSingle<IUserService, UserService>();
            Container.RegisterSingle<IPermissionService, PermissionService>();
            Container.RegisterSingle<ILog>(new LogImpl(LoggerManager.CreateRepository(typeof(App).FullName).GetLogger(typeof(App).Name)));

            Container.Register<MainWindowViewModel>();
            Container.Register<PermissionViewModel>();
            Container.Register<PermissionsTreeViewModel>();
            Container.Register<UserAccountViewModel>();
            Container.Register<UserEditorViewModel>();
            Container.Register<UserManagerViewModel>();
            Container.Register<UserViewModel>();

            MainWindow = new MainWindow { DataContext = Container.GetInstance<MainWindowViewModel>() };
            MainWindow.Show();
        }
    }
}
