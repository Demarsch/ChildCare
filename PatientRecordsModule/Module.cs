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
            container.RegisterType<NewVisitCreatingViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            container.RegisterType<object, PersonRecords>(viewNameResolver.Resolve<PersonRecordsViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordList>(viewNameResolver.Resolve<PersonRecordListViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PersonRecordEditor>(viewNameResolver.Resolve<PersonRecordEditorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, NewVisitCreating>(viewNameResolver.Resolve<NewVisitCreatingViewModel>(), new ContainerControlledLifetimeManager());
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelectedAsync);
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<PersonRecordsHeader>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PersonRecords>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/PatientRecordsModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private async void OnPatientSelectedAsync(int patientId)
        {
            DbContext context = null;
            var ribbonContextualGroup = container.Resolve<RibbonContextualTabGroup>(Common.RibbonGroupName);
            try
            {
                context = contextProvider.CreateNewContext();
                var data = await context.Set<Person>()
                                        .Where(x => x.Id == patientId)
                                        .Select(x => new { x.ShortName, x.GenderId })
                                        .FirstOrDefaultAsync();
                if (data == null)
                {
                    ribbonContextualGroup.Header = PatientIsNotSelected;
                    log.ErrorFormat("Patient with Id {0} not found in database", patientId);
                }
                else
                {
                    ribbonContextualGroup.Header = string.Format("{0} {1}",
                                                                 data.GenderId == Gender.MaleGenderId ? "Пациент" : "Пациентка",
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
            }
        }

        private void RegisterServices()
        {
            container.RegisterType<IPatientRecordsService, PatientRecordsService>(new ContainerControlledLifetimeManager());
        }
        #endregion

    }
}
