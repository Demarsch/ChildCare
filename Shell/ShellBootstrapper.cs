using System.Windows;
using Core.Data.Services;
using Core.Services;
using Fluent;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;

namespace Shell
{
    public class ShellBootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<ShellWindow>();
        }

        protected override async void InitializeShell()
        {
            var shellWindow = (ShellWindow)Shell;
            Application.Current.MainWindow = shellWindow;
            Application.Current.MainWindow.Show();
            await shellWindow.ShellWindowViewModel.CheckDatabaseConnectionAsync();
            base.InitializeModules();
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var result = base.ConfigureRegionAdapterMappings();
            result.RegisterMapping(typeof(Ribbon), Container.Resolve<RibbonRegionAdapter>());
            return result;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            RegisterServices();
        }

        protected override void InitializeModules()
        {
            //Do nothing as we actually still wait for connection status at this point
        }

        private void RegisterServices()
        {
            Container.RegisterType<IDbContextProvider, DbContextProvider>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICacheService, DbContextCacheService>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(LogManager.GetLogger("SHELL"));
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog { ModulePath = @".\" };
        }
    }
}
