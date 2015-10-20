using System;
using log4net;
using Microsoft.Practices.Unity;
using PatientInfoModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;

namespace PatientInfoModule
{
    [Module(ModuleName = WellKnownModuleNames.PatientInfoModule)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        public Module(IUnityContainer container, IRegionManager regionManager)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            this.container = container;
            this.regionManager = regionManager;
        }

        public void Initialize()
        {
            RegisterServices();
            RegisterViews();
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<ModuleHeader>());
        }

        private void RegisterServices()
        {
            container.RegisterInstance(LogManager.GetLogger("PATINFO"));
        }
    }
}