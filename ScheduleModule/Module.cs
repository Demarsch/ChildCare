using System;
using System.Windows;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using ScheduleModule.Services;
using ScheduleModule.ViewModels;
using ScheduleModule.Views;
using Shell.Shared;

namespace ScheduleModule
{
    [Module(ModuleName = WellKnownModuleNames.ScheduleModule, OnDemand = true)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly ILog log;

        public Module(IUnityContainer container, IRegionManager regionManager, IViewNameResolver viewNameResolver, ILog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
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
            this.log = log;
        }

        public void Initialize()
        {
            log.InfoFormat("{0} module init start", WellKnownModuleNames.ScheduleModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.ScheduleModule);
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
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, typeof(ScheduleHeaderView));
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/ScheduleModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IScheduleService, ScheduleService>(new ContainerControlledLifetimeManager());
        }
    }
}
