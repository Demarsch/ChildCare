﻿using System;
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
using WpfControls.Editors;
using PatientRecordsModule.ViewModels.RecordTypesProtocolViewModels;
using PatientRecordsModule.Views.RecordTypesProtocolViews;

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
            container.RegisterType<VisitCloseViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<BrigadeViewModel>(new ContainerControlledLifetimeManager());
            //RecordDocuments
            container.RegisterType<RecordDocumentsCollectionViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<RecordDocumentViewModel>(new ContainerControlledLifetimeManager());
            //RecordTypes Protocols
            container.RegisterType<DefaultProtocolViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            container.RegisterType<object, PersonRecordsView>(viewNameResolver.Resolve<PersonRecordsViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordListView>(viewNameResolver.Resolve<PersonRecordListViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordEditorView>(viewNameResolver.Resolve<PersonRecordEditorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, VisitEditorView>(viewNameResolver.Resolve<VisitEditorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, VisitCloseView>(viewNameResolver.Resolve<VisitCloseViewModel>(), new ContainerControlledLifetimeManager());
            //RecordDocuments
            container.RegisterType<object, RecordDocumentsView>(viewNameResolver.Resolve<RecordDocumentsCollectionViewModel>(), new ContainerControlledLifetimeManager());
            //RecordTypes Protocols
            container.RegisterType<object, DefaultProtocolView>(viewNameResolver.Resolve<DefaultProtocolViewModel>(), new ContainerControlledLifetimeManager());


            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<PersonRecordsHeader>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PersonRecordsView>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/PatientRecordsModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IPatientRecordsService, PatientRecordsService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDocumentService, DocumentService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRecordService, RecordService>(new ContainerControlledLifetimeManager());

            container.RegisterType<ISuggestionProvider, MKBSuggestionProvider>(SuggestionProviderNames.MKB, new ContainerControlledLifetimeManager());
        }
        #endregion

    }
}
