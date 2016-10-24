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
using UserMessageModule.Services;
using UserMessageModule.ViewModels;
using UserMessageModule.Views;
using Core.Data;

namespace UserMessageModule
{
    [Module(ModuleName = WellKnownModuleNames.UserMessageModule, OnDemand = true)]
    [PermissionRequired(Permission.UserMessageModuleAccess)]
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
            var log = LogManager.GetLogger("USERMESSAGE");
            container.RegisterInstance(log);
            log.InfoFormat("{0} module init start", WellKnownModuleNames.UserMessageModule);

            //services
            container.RegisterType<IUserMessageService, UserMessageService>(new ContainerControlledLifetimeManager());

            //viewmodels
            container.RegisterType<MessageHeaderViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<MessageInboxViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<MessageSelectorViewModel>(new ContainerControlledLifetimeManager());
            
            //views
            container.RegisterType<object, MessageSelectorView>(viewNameResolver.Resolve<MessageSelectorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, MessageInboxView>(viewNameResolver.Resolve<MessageInboxViewModel>(), new ContainerControlledLifetimeManager());

            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<MessageHeaderView>());

            regionManager.RegisterViewWithRegion(RegionNames.ListItems, () => container.Resolve<MessageSelectorView>());
            regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<MessageInboxView>());

            log.InfoFormat("{0} module init finished", WellKnownModuleNames.UserMessageModule);
        }

    }
}
