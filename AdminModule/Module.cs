using System;
using System.Windows;
using AdminModule.Services;
using Core.Data;
using Prism.Modularity;
using Shared.Patient.Misc;
using Shared.Patient.Services;
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
    [Module(ModuleName = WellKnownModuleNames.AdminModule, OnDemand = true)]
    [PermissionRequired(Permission.AdminModuleAccess)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

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
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.AdminModule);
        }

        private void RegisterServices()
        {
            CoreReports.Initialize(container);
            PersonServicesInitializer.Initialize(container);
            container.RegisterType<IPersonSearchService, Services.PersonSearchService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUserAccessService, UserAccessService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUserProvider, ActiveDirectoryUserProvider>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViewModels()
        {
            container.RegisterType<GroupEditDialogViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<AdminEmptyViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<ReportTemplatesManagerViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<AdminHeaderViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            container.RegisterType<object, AdminEmptyView>(viewNameResolver.Resolve<AdminEmptyViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<AdminEmptyView>());
            container.RegisterType<object, ReportTemplatesManagerView>(viewNameResolver.Resolve<ReportTemplatesManagerViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<ReportTemplatesManagerView>());
            container.RegisterType<AdminHeaderView>(new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<AdminHeaderView>());
            container.RegisterType<object, UserAccessManagerView>(viewNameResolver.Resolve<UserAccessManagerViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<UserAccessManagerView>());

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/AdminModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }
    }
}
