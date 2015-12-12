using System;
using Core.Data;
using Core.Data.Services;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using OrganizationContractsModule.Services;
using OrganizationContractsModule.ViewModels;
using OrganizationContractsModule.Views;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;

namespace OrganizationContracts
{
    [Module(ModuleName = WellKnownModuleNames.OrganizationContractsModule, OnDemand = true)]
    [PermissionRequired(Permission.OrganizationContractsModuleAccess)]
    public class Module : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IDbContextProvider contextProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly IViewNameResolver viewNameResolver;

        private readonly ILog log;
        #endregion

        #region Constructors
        public Module(IUnityContainer container,
                     IRegionManager regionManager,
                     IDbContextProvider contextProvider,
                     IViewNameResolver viewNameResolver,
                     IEventAggregator eventAggregator,
                     ILog log)
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
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.container = container;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.contextProvider = contextProvider;
            this.eventAggregator = eventAggregator;
            this.log = log;
        }
        #endregion

        #region Methods
        public void Initialize()
        {
            log.InfoFormat("{0} module init start", WellKnownModuleNames.OrganizationContractsModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.OrganizationContractsModule);
        }

        private void RegisterViewModels()
        {
            container.RegisterType<OrgContractsViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            container.RegisterType<object, OrgContractsView>(viewNameResolver.Resolve<OrgContractsViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<OrgContractsHeaderView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<OrgContractsView>());    
        }
       
        private void RegisterServices()
        {
            container.RegisterType<IContractService, ContractService>(new ContainerControlledLifetimeManager());
        }
        #endregion

    }
}
