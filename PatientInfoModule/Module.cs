using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Services;
using Fluent;
using log4net;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using PatientInfoModule.ViewModels;
using PatientInfoModule.Views;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;
using Shared.PatientRecords;

namespace PatientInfoModule
{
    [Module(ModuleName = WellKnownModuleNames.PatientInfoModule, OnDemand = true)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    [PermissionRequired(Permission.PatientInfoModuleAccess)]
    public class Module : IModule
    {
        private const string PatientIsNotSelected = "Пациент не выбран";

        private const string NewPatient = "Новый пациент";

        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly IDbContextProvider contextProvider;

        private readonly IEventAggregator eventAggregator;

        private ILog log;

        public Module(IUnityContainer container,
                      IRegionManager regionManager,
                      IViewNameResolver viewNameResolver,
                      IDbContextProvider contextProvider,
                      IEventAggregator eventAggregator)
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
            this.container = container.CreateChildContainer();
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.contextProvider = contextProvider;
            this.eventAggregator = eventAggregator;
        }

        public void Initialize()
        {
            RegisterLogger();
            log.InfoFormat("{0} module init start", WellKnownModuleNames.PatientInfoModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            InitiateLongRunningOperations();
            var patientRecords = container.Resolve<PatientRecords>();
            patientRecords.Initialize();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.PatientInfoModule);
        }

        private void RegisterViewModels()
        {
            container.RegisterType<EmptyPatientInfoViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<InfoContentViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PatientContractsViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<PersonDocumentsViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            container.RegisterType<object, EmptyPatientInfoView>(viewNameResolver.Resolve<EmptyPatientInfoViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<EmptyPatientInfoView>());
            container.RegisterType<object, InfoContentView>(viewNameResolver.Resolve<InfoContentViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<InfoContentView>());
            container.RegisterType<object, PatientContractsView>(viewNameResolver.Resolve<PatientContractsViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PatientContractsView>());
            container.RegisterType<object, PersonDocumentsView>(viewNameResolver.Resolve<PersonDocumentsViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PersonDocumentsView>());
            container.RegisterInstance(Common.RibbonGroupName,
                                       new RibbonContextualTabGroup
                                       {
                                           Visibility = Visibility.Visible,
                                           Background = Brushes.SteelBlue,
                                           BorderBrush = Brushes.Blue,
                                           Header = PatientIsNotSelected
                                       });
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Subscribe(OnPatientSelectedAsync, true);
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<InfoHeaderView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<DocumentsHeaderView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<ContractsHeaderView>());

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/PatientInfoModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private async void OnPatientSelectedAsync(int patientId)
        {
            DbContext context = null;
            var ribbonContextualGroup = container.Resolve<RibbonContextualTabGroup>(Common.RibbonGroupName);
            if (patientId == SpecialValues.NonExistingId)
            {
                ribbonContextualGroup.Header = PatientIsNotSelected;
                return;
            }
            if (patientId == SpecialValues.NewId)
            {
                ribbonContextualGroup.Header = NewPatient;
                return;
            }
            try
            {
                context = contextProvider.CreateNewContext();
                var data = await context.Set<Person>()
                                        .Where(x => x.Id == patientId)
                                        .Select(x => new { x.ShortName, x.IsMale })
                                        .FirstOrDefaultAsync();
                if (data == null)
                {
                    ribbonContextualGroup.Header = PatientIsNotSelected;
                    log.ErrorFormat("Patient with Id {0} not found in database", patientId);
                }
                else
                {
                    ribbonContextualGroup.Header = string.Format("{0} {1}",
                                                                 data.IsMale ? "Пациент" : "Пациентка",
                                                                 data.ShortName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to retrieve patient short name for ribbon contextual group header", ex);
                ribbonContextualGroup.Header = PatientIsNotSelected;
            }
            finally
            {
                if (context != null)
                {
                    context.Dispose();
                }
                var activeRibbonTabItem = regionManager.Regions[RegionNames.ModuleList].ActiveViews.FirstOrDefault() as RibbonTabItem;
                if (activeRibbonTabItem == null || !ReferenceEquals(activeRibbonTabItem.Group, ribbonContextualGroup))
                {
                    regionManager.Regions[RegionNames.ModuleList].RequestNavigate(viewNameResolver.Resolve<InfoHeaderViewModel>());
                }
            }
        }

        private void RegisterServices()
        {

            container.RegisterType<IPatientService, PatientService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRecordService, RecordService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContractService, ContractService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAssignmentService, AssignmentService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDocumentService, DocumentService>(new ContainerControlledLifetimeManager());

            container.RegisterType<ISuggestionsProvider, IdentityDocumentGivenOrgSuggestionsProvider>(SuggestionProviderNames.IdentityDocumentGiveOrganization, new ContainerControlledLifetimeManager());
            container.RegisterType<ISuggestionsProvider, InsuranceCompanySuggestionsProvider>(SuggestionProviderNames.InsuranceCompany, new ContainerControlledLifetimeManager());
            container.RegisterType<ISuggestionsProvider, PersonSuggestionsProvider>(SuggestionProviderNames.Person, new ContainerControlledLifetimeManager());
            container.RegisterType<ISuggestionsProvider, OkatoRegionSuggestionsProvider>(SuggestionProviderNames.OkatoRegion, new ContainerControlledLifetimeManager());
            container.RegisterType<ISuggestionsProvider, DisabilityDocumentGivenOrgSuggestionsProvider>(SuggestionProviderNames.DisabilityDocumentGivenOrganization, new ContainerControlledLifetimeManager());
            container.RegisterType<ISuggestionsProvider, OrganizationSuggestionsProvider>(SuggestionProviderNames.Organization, new ContainerControlledLifetimeManager());
            container.RegisterType<IAddressSuggestionProvider, AddressSuggestionProvider>(new ContainerControlledLifetimeManager());
        }

        private void RegisterLogger()
        {
            log = LogManager.GetLogger("PATINFO");
            container.RegisterInstance(log);
        }

        private void InitiateLongRunningOperations()
        {
            var addressSuggestionProvider = container.Resolve<IAddressSuggestionProvider>();
            addressSuggestionProvider.EnsureDataSourceLoadedAsync();
        }
    }
}