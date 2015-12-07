﻿using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Modularity;
using Shell.Shared;
using Microsoft.Practices.Unity;
using log4net;
using Prism.Regions;
using AdminModule.Views;
using AdminModule.ViewModels;
using Core.Wpf.Services;
using Core.Reports;

namespace AdminModule
{
    [Module(ModuleName = WellKnownModuleNames.AdminModule)]
    public class Module : IModule
    {
        IUnityContainer container;
        IRegionManager regionManager;
        IViewNameResolver viewNameResolver;

        public Module(IUnityContainer container, IRegionManager regionManager, IViewNameResolver viewNameResolver)
        {
            this.container = container.CreateChildContainer();
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }

        public void Initialize()
        {
            container.RegisterInstance(LogManager.GetLogger("ADMINING"));
            var log = container.Resolve<ILog>();
            log.InfoFormat("{0} module init start", WellKnownModuleNames.AdminModule);
            CoreReports.Initialize(container);

            container.RegisterType<AdminEmptyViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<object, AdminEmptyView>(viewNameResolver.Resolve<AdminEmptyViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<AdminEmptyView>());

            container.RegisterType<ReportTemplateEditorViewModel>(new TransientLifetimeManager());
            container.RegisterType<ReportTemplatesManagerViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<object, ReportTemplatesManagerView>(viewNameResolver.Resolve<ReportTemplatesManagerViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<ReportTemplatesManagerView>());

            // header
            container.RegisterType<AdminHeaderViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<AdminHeaderView>(new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<AdminHeaderView>());
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.AdminModule);
        }
    }
}
