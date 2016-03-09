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
using CommissionsModule.ViewModels;
using CommissionsModule.Views;
using CommissionsModule.Services;
using System.Windows;
using Shared.Patient.Misc;
using Shared.PatientRecords.Services;
using Core.Wpf.Misc;
using Shared.Patient.Views;
using Shared.Patient.ViewModels;

namespace CommissionsModule
{
    [Module(ModuleName = WellKnownModuleNames.CommissionsModule, OnDemand = true)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    [PermissionRequired(Permission.CommissionsModuleAccess)]
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
            log.InfoFormat("{0} module init start", WellKnownModuleNames.CommissionsModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.CommissionsModule);
        }

        private void RegisterLogger()
        {
            log = LogManager.GetLogger("COMMISSIONS");
            container.RegisterInstance(log);
        }

        private void RegisterViewModels()
        {
            container.RegisterType<CommissionEmptyViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<CommissionProtocolHeaderViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<CommissionDecisionHeaderViewModel>(new ContainerControlledLifetimeManager());

            container.RegisterType<CommissionsListViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<CommissionDecisionsViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<CommissionDecisionEditorViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<CommissionDecisionViewModel>(new TransientLifetimeManager());
            container.RegisterType<CommissionProtocolViewModel>(new TransientLifetimeManager());
            container.RegisterType<PreliminaryProtocolViewModel>(new TransientLifetimeManager());
            container.RegisterType<CommissionСonductViewModel>(new TransientLifetimeManager());
            container.RegisterType<CommissionСonclusionViewModel>(new TransientLifetimeManager());
            container.RegisterType<CommissionMemberViewModel>(new TransientLifetimeManager());

            container.RegisterType<AddressViewModel>(new TransientLifetimeManager());
        }

        private void RegisterViews()
        {
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<CommissionProtocolHeaderView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<CommissionDecisionHeaderView>());            

            container.RegisterType<object, CommissionEmptyView>(viewNameResolver.Resolve<CommissionEmptyViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionsListView>(viewNameResolver.Resolve<CommissionsListViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionDecisionsView>(viewNameResolver.Resolve<CommissionDecisionsViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionDecisionView>(viewNameResolver.Resolve<CommissionDecisionViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionDecisionEditorView>(viewNameResolver.Resolve<CommissionDecisionEditorViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionProtocolView>(viewNameResolver.Resolve<CommissionProtocolViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionСonclusionView>(viewNameResolver.Resolve<CommissionСonclusionViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, CommissionСonductView>(viewNameResolver.Resolve<CommissionСonductViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, PreliminaryProtocolView>(viewNameResolver.Resolve<PreliminaryProtocolViewModel>(), new ContainerControlledLifetimeManager());

            container.RegisterType<object, AddressView>(viewNameResolver.Resolve<AddressViewModel>(), new ContainerControlledLifetimeManager());

            regionManager.RegisterViewWithRegion(RegionNames.ListItems, () => container.Resolve<CommissionsListView>());
            regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<CommissionDecisionsView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<CommissionProtocolView>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<CommissionEmptyView>());

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/CommissionsModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<ICommissionService, CommissionService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDiagnosService, DiagnosService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISuggestionsProvider, OkatoRegionSuggestionsProvider>(SuggestionProviderNames.OkatoRegion, new ContainerControlledLifetimeManager());
            container.RegisterType<IAddressSuggestionProvider, AddressSuggestionProvider>(new ContainerControlledLifetimeManager());
        }
    }
}

