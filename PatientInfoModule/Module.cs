using System;
using Core.Wpf.Services;
using Fluent;
using log4net;
using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;
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

        private readonly IViewNameResolver viewNameResolver;

        public Module(IUnityContainer container, IRegionManager regionManager, IViewNameResolver viewNameResolver)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            this.container = container;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }

        public void Initialize()
        {
            RegisterServices();
            RegisterViews();
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            container.RegisterType<object, EmptyPatientInfo>(viewNameResolver.Resolve<EmptyPatientInfoViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PatientInfo>(viewNameResolver.Resolve<PatientInfoViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<ModuleHeader>());
        }

        private void RegisterServices()
        {
            container.RegisterInstance(LogManager.GetLogger("PATINFO"));
        }
    }
}