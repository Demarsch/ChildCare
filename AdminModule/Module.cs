using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Modularity;
using Shell.Shared;
using Microsoft.Practices.Unity;
using log4net;
using Prism.Regions;
using AdminModule.Views;
using AdminModule.ViewModels;

namespace AdminModule
{
    [Module(ModuleName = WellKnownModuleNames.AdminModule)]
    public class Module : IModule
    {
        IUnityContainer container;
        IRegionManager regionManager;

        public Module(IUnityContainer container, IRegionManager regionManager)
        {
            this.container = container.CreateChildContainer();
            this.regionManager = regionManager;
        }

        public void Initialize()
        {
            //services
            container.RegisterInstance(LogManager.GetLogger("ADMINING"));

            container.RegisterType<AdminViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<AdminHeader>(new ContainerControlledLifetimeManager());
        }
    }
}
