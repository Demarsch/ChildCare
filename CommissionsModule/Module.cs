using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism;
using Prism.Modularity;
using Shell.Shared;
using Core.Data;
using Prism.Regions;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels;
using CommissionsModule.Views;
using CommissionsModule.Services;

namespace CommissionsModule
{
    [Module(ModuleName = WellKnownModuleNames.CommissionsModule, OnDemand = true)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    [PermissionRequired(Permission.CommissionsModuleAccess)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private ILog log;

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
            this.container = container.CreateChildContainer();
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }

        public void Initialize()
        {
            RegisterLogger();
            log.InfoFormat("{0} module init start", WellKnownModuleNames.CommissionsModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.CommissionsModule);
        }

        private void RegisterLogger()
        {
            log = LogManager.GetLogger("COMMISSIONS");
            container.RegisterInstance(log);
        }

        private void RegisterViewModels()
        {
            container.RegisterType<CommissionsHeaderViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<CommissionDecisionHeaderViewModel>(new ContainerControlledLifetimeManager());

            container.RegisterType<CommissionsListViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<CommissionDecisionViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<CommissionsHeaderView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<CommissionDecisionHeaderView>());

            container.RegisterType<object, CommissionsListView>(viewNameResolver.Resolve<CommissionsListViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionDecisionView>(viewNameResolver.Resolve<CommissionDecisionViewModel>(), new ContainerControlledLifetimeManager());

            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<CommissionsListView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<CommissionDecisionView>());
        }

        private void RegisterServices()
        {
            container.RegisterType<ICommissionService, CommissionService>(new ContainerControlledLifetimeManager());
        }
    }
}

