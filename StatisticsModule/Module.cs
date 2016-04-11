using System;
using Core.Data;
using Core.Data.Services;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;
using System.Windows;
using StatisticsModule.ViewModels;
using StatisticsModule.Views;
using StatisticsModule.Services;

namespace StatisticsModule
{
    [Module(ModuleName = WellKnownModuleNames.StatisticsModule, OnDemand = true)]
    [PermissionRequired(Permission.StatisticsModuleAccess)]
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
            log.InfoFormat("{0} module init start", WellKnownModuleNames.StatisticsModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.StatisticsModule);
        }

        private void RegisterViewModels()
        {
            container.RegisterType<StatisticsHeaderViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<StatisticsEmptyViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<RecordsStatisticsViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            container.RegisterType<StatisticsHeaderView>(new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<StatisticsHeaderView>());

            container.RegisterType<object, StatisticsEmptyView>(viewNameResolver.Resolve<StatisticsEmptyViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<StatisticsEmptyView>());

            container.RegisterType<object, RecordsStatisticsView>(viewNameResolver.Resolve<RecordsStatisticsViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<RecordsStatisticsView>());
            
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/StatisticsModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }
       
        private void RegisterServices()
        {
            container.RegisterType<IStatisticsService, StatisticsService>(new ContainerControlledLifetimeManager());
        }
        #endregion

    }
}
