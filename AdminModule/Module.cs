using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AdminModule.Model;
using AdminModule.Services;
using AdminModule.ViewModels;
using AdminModule.Views;
using Core.Data;
using Core.Reports;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using Shared.Patient.Misc;
using Shared.Patient.Services;
using Shell.Shared;
using PersonSearchService = AdminModule.Services.PersonSearchService;

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
            container.RegisterType<IPersonSearchService, PersonSearchService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUserAccessService, UserAccessService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUserProvider, ActiveDirectoryUserProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDatabaseValidtionService, DatabaseValidationService>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(GetAllDatabaseValidators());
        }

        private IEnumerable<IDatabaseValidator> GetAllDatabaseValidators()
        {
            var result = typeof(Module).Assembly.GetTypes().Where(x => x.IsClass && typeof(IDatabaseValidator).IsAssignableFrom(x))
                                       .Select(x => container.Resolve(x))
                                       .Cast<IDatabaseValidator>()
                                       .ToArray();
            return result;
        }

        private void RegisterViewModels()
        {
            container.RegisterType<GroupEditDialogViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<AdminEmptyViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<ReportTemplatesManagerViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<AdminHeaderViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<UserAccessManagerViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<DatabaseValidationViewModel>(new ContainerControlledLifetimeManager());
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
            container.RegisterType<object, DatabaseValidationView>(viewNameResolver.Resolve<DatabaseValidationViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<DatabaseValidationView>());

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/AdminModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }
    }
}