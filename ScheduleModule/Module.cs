using System;
using System.Windows;
using Core.Data;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using ScheduleModule.Services;
using ScheduleModule.ViewModels;
using ScheduleModule.Views;
using Shared.Patient.Misc;
using Shell.Shared;

namespace ScheduleModule
{
    [Module(ModuleName = WellKnownModuleNames.ScheduleModule, OnDemand = true)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    [PermissionRequired(Permission.ScheduleModuleAccess)]
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
            log.InfoFormat("{0} module init start", WellKnownModuleNames.ScheduleModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.ScheduleModule);
        }

        private void RegisterLogger()
        {
            log = LogManager.GetLogger("SCHEDULE");
            container.RegisterInstance(log);
        }

        private void RegisterViewModels()
        {
            container.RegisterType<ScheduleAssignmentUpdateViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<TimeTickerViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<ScheduleContentViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            container.RegisterType<object, ScheduleContentView>(viewNameResolver.Resolve<ScheduleContentViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<ScheduleContentView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<ScheduleHeaderView>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/ScheduleModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IScheduleService, ScheduleService>(new ContainerControlledLifetimeManager());
            PersonServicesInitializer.Initialize(container);
        }
    }
}
