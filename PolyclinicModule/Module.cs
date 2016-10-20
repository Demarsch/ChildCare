using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Wpf.Extensions;
using Prism;
using Prism.Modularity;
using Shell.Shared;
using Core.Data;
using Prism.Regions;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using System.Windows;
using Core.Wpf.Misc;
using Core.Reports;
using Shared.PatientRecords.ViewModels;
using Shared.PatientRecords.Views;
using Prism.Events;
using Core.Data.Services;
using Shared.PatientRecords;
using PolyclinicModule.ViewModels;
using PolyclinicModule.Views;
using PolyclinicModule.Services;

namespace PolyclinicModule
{
    [Module(ModuleName = WellKnownModuleNames.PolyclinicModule, OnDemand = true)]
    [PermissionRequired(Permission.PolyclinicModuleAccess)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly IEventAggregator eventAggregator;

        private readonly IDbContextProvider contextProvider;

        private ILog log;

        public Module(IUnityContainer container, IRegionManager regionManager, IViewNameResolver viewNameResolver, IEventAggregator eventAggregator, IDbContextProvider provider)
        {
            this.container = container.CreateChildContainer();
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.viewNameResolver = viewNameResolver;
            this.contextProvider = provider;
        }

        public void Initialize()
        {
            RegisterLogger();
            log.InfoFormat("{0} module init start", WellKnownModuleNames.PolyclinicModule);
            RegisterModules();
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.PolyclinicModule);
        }

        private void RegisterModules()
        {
            CoreReports.Initialize(container);
            PatientRecords.Initialize(container, regionManager, contextProvider, viewNameResolver, eventAggregator, log);
        }

        private void RegisterLogger()
        {
            log = LogManager.GetLogger("POLYCLINIC");
            container.RegisterInstance(log);
        }

        private void RegisterServices()
        {
            container.RegisterType<IPolyclinicService, PolyclinicService>(new ContainerControlledLifetimeManager());
        } 

        private void RegisterViewModels()
        {
            container.RegisterType<PolyclinicEmptyViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PolyclinicPersonListViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<PolyclinicHeaderView>());

            container.RegisterType<object, PolyclinicEmptyView>(viewNameResolver.Resolve<PolyclinicEmptyViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PolyclinicPersonListView>(viewNameResolver.Resolve<PolyclinicPersonListViewModel>(), new ContainerControlledLifetimeManager());

            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PolyclinicEmptyView>());
            regionManager.RegisterViewWithRegion(RegionNames.ListItems, () => container.Resolve<PolyclinicPersonListView>());            
            regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/PolyclinicModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
             
        }
    }
}