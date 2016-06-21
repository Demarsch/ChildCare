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
using Shared.PatientRecords.Misc;
using Core.Data;
using System.Data.Entity;
using Shared.PatientRecords.ViewModels;
using Shared.PatientRecords.Views;
using Shared.PatientRecords.Services;
using Core.Wpf.Events;
using System.Windows;
using WpfControls.Editors;
using Shared.PatientRecords.ViewModels.RecordTypesProtocolViewModels;
using Shared.PatientRecords.Views.RecordTypesProtocolViewModels;
using Shared.PatientRecords.ViewModels.PersonHierarchicalItemViewModels;

namespace Shared.PatientRecords
{
    public class PatientRecords
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
        public PatientRecords(IUnityContainer container,
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
            log.InfoFormat("Patient records library start");
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("Patient records library finished");
        }

        private void RegisterViewModels()
        {
            container.RegisterType<PersonRecordsViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonRecordListViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonRecordEditorViewModel>(new TransientLifetimeManager());
            container.RegisterType<PersonHierarchicalAssignmentsViewModel>(new TransientLifetimeManager());
            container.RegisterType<PersonHierarchicalVisitsViewModel>(new TransientLifetimeManager());
            container.RegisterType<PersonHierarchicalRecordsViewModel>(new TransientLifetimeManager());
            container.RegisterType<VisitEditorViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<VisitCloseViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<RecordCreateViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<BrigadeViewModel>(new ContainerControlledLifetimeManager());
            //RecordDocuments
            container.RegisterType<RecordDocumentsCollectionViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<RecordDocumentViewModel>(new ContainerControlledLifetimeManager());
            //Diagnoses
            container.RegisterType<DiagnosesCollectionViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<DiagnosViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<DiagnosLevelViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<MKBTreeViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<MKBViewModel>(new ContainerControlledLifetimeManager());            
            container.RegisterType<ComplicationsTreeViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<ComplicationViewModel>(new ContainerControlledLifetimeManager());
            //Analyses
            container.RegisterType<AnalyseCreateViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<AnalyseProtocolViewModel>(new TransientLifetimeManager());
            //RecordTypes Protocols
            container.RegisterType<DefaultProtocolViewModel>(new TransientLifetimeManager());
            container.RegisterType<VisitProtocolViewModel>(new TransientLifetimeManager());
        }

        private void RegisterViews()
        {
            container.RegisterType<object, PersonRecordsView>(viewNameResolver.Resolve<PersonRecordsViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordListView>(viewNameResolver.Resolve<PersonRecordListViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordEditorView>(viewNameResolver.Resolve<PersonRecordEditorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, VisitEditorView>(viewNameResolver.Resolve<VisitEditorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, VisitCloseView>(viewNameResolver.Resolve<VisitCloseViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, RecordCreateView>(viewNameResolver.Resolve<RecordCreateViewModel>(), new ContainerControlledLifetimeManager());
            //RecordDocuments
            container.RegisterType<object, RecordDocumentsView>(viewNameResolver.Resolve<RecordDocumentsCollectionViewModel>(), new ContainerControlledLifetimeManager());
            //Diagnoses
            container.RegisterType<object, DiagnosesView>(viewNameResolver.Resolve<DiagnosesCollectionViewModel>(), new ContainerControlledLifetimeManager());
            //Analyses
            container.RegisterType<object, DiagnosesView>(viewNameResolver.Resolve<DiagnosesCollectionViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, AnalyseProtocolView>(viewNameResolver.Resolve<AnalyseProtocolViewModel>(), new ContainerControlledLifetimeManager());
            //RecordTypes Protocols
            container.RegisterType<object, DefaultProtocolView>(viewNameResolver.Resolve<DefaultProtocolViewModel>(), new ContainerControlledLifetimeManager());


            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<PersonRecordsHeader>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PersonRecordsView>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/Shared.PatientRecords;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IHierarchicalRepository, HierarchicalRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPersonRecordEditor, PersonRecordEditorViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPatientRecordsService, PatientRecordsService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDocumentService, DocumentService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDiagnosService, DiagnosService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRecordTypeEditorResolver, RecordTypeEditorResolver>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUserService, UserService>(new ContainerControlledLifetimeManager());

            container.RegisterType<ISuggestionProvider, MKBSuggestionProvider>(SuggestionProviderNames.MKB, new ContainerControlledLifetimeManager());
        }
        #endregion

    }
}

