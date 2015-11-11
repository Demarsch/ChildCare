using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;
using log4net;
using Microsoft.Practices.Unity;
using Core.Data.Services;
using Core.Wpf.Services;
using Fluent;
using PatientRecordsModule.Misc;
using Core.Data;
using System.Data.Entity;
using PatientRecordsModule.ViewModels;
using PatientRecordsModule.Views;
using PatientRecordsModule.Services;
using Core.Wpf.Events;
using System.Windows;

namespace PatientRecordsModule
{
    [Module(ModuleName = WellKnownModuleNames.PatientRecordsModule)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    public class Module : IModule
    {
        #region Fields
        private const string PatientIsNotSelected = "Пациент не выбран";

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
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
        }

        private void RegisterViewModels()
        {
            container.RegisterType<PersonRecordsViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonRecordListViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonRecordEditorViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonHierarchicalAssignmentsViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonHierarchicalVisitsViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonHierarchicalRecordsViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<VisitEditorViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            container.RegisterType<object, PersonRecordsView>(viewNameResolver.Resolve<PersonRecordsViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordListView>(viewNameResolver.Resolve<PersonRecordListViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordEditorView>(viewNameResolver.Resolve<PersonRecordEditorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, VisitEditorViewModel>(viewNameResolver.Resolve<VisitEditorViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<PersonRecordsHeader>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PersonRecordsView>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/PatientRecordsModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IPatientRecordsService, PatientRecordsService>(new ContainerControlledLifetimeManager());
        }
        #endregion

    }
}
