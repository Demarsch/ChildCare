using System.Windows;
using Fluent;
using log4net;
using log4net.Util;
using Microsoft.Practices.Unity;
using Prism.Logging;
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

        protected override void InitializeShell()
        {
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
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
            Container.RegisterInstance(LogManager.GetLogger("SHELL"));
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog { ModulePath = @".\" };
        }
    }
}
