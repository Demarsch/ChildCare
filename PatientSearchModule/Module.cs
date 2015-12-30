using System;
using System.Windows;
using log4net;
using Microsoft.Practices.Unity;
using PatientSearchModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Shared.Patient.Misc;
using Shell.Shared;

namespace PatientSearchModule
{
    [Module(ModuleName = WellKnownModuleNames.PatientSearchModule, OnDemand = true)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IUnityContainer globalContainer;

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
            this.regionManager = regionManager;
            globalContainer = container;
            this.container = container.CreateChildContainer();
        }

        public void Initialize()
        {
            RegisterLoger();
            var log = container.Resolve<ILog>();
            log.InfoFormat("{0} module init start", WellKnownModuleNames.PatientSearchModule);
            RegisterServices();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.PatientSearchModule);
        }

        private void RegisterServices()
        {
            PersonServicesInitializer.Initialize(globalContainer);
        }

        private void RegisterLoger()
        {
            container.RegisterInstance(LogManager.GetLogger("PATSEARCH"));
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, () => container.Resolve<PatientSearchView>());
        }
    }
}