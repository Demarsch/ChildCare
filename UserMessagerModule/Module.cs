using Core.Wpf.Services;
using Core.Wpf.Extensions;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UserMessagerModule.Services;
using UserMessagerModule.ViewModels;
using UserMessagerModule.Views;
using Core.Data;

namespace UserMessagerModule
{
    [Module(ModuleName = WellKnownModuleNames.UserMessagerModule, OnDemand = true)]
    [PermissionRequired(Permission.UserMessagerModuleAccess)]
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
            this.container = container.CreateChildContainer();
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }

        public void Initialize()
        {
            var log = LogManager.GetLogger("USERMESSAGER");
            container.RegisterInstance(log);
            log.InfoFormat("{0} module init start", WellKnownModuleNames.UserMessagerModule);

            //services
            container.RegisterType<IUserMessageService, UserMessageService>(new ContainerControlledLifetimeManager());

            //viewmodels
            container.RegisterType<MessagerHeaderViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<MessagerInboxViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<MessagerSelectorViewModel>(new ContainerControlledLifetimeManager());
            
            //views
            container.RegisterType<object, MessagerSelectorView>(viewNameResolver.Resolve<MessagerSelectorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, MessagerInboxView>(viewNameResolver.Resolve<MessagerInboxViewModel>(), new ContainerControlledLifetimeManager());

            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<MessagerHeaderView>());

            regionManager.RegisterViewWithRegion(RegionNames.ListItems, () => container.Resolve<MessagerSelectorView>());
            regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<MessagerInboxView>());

            log.InfoFormat("{0} module init finished", WellKnownModuleNames.UserMessagerModule);
        }

    }
}
