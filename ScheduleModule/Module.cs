using System;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Shell.Shared;

namespace ScheduleModule
{
    [Module(ModuleName = WellKnownModuleNames.ScheduleModule)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        public Module(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
        }

        public void Initialize()
        {
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            InitiateLongRunningOperations();
        }

        private void RegisterViewModels()
        {
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            //container.RegisterType<object, EmptyPatientInfoView>(viewNameResolver.Resolve<EmptyPatientInfoViewModel>(), new ContainerControlledLifetimeManager());
            //container.RegisterType<object, InfoContentView>(viewNameResolver.Resolve<InfoContentViewModel>(), new ContainerControlledLifetimeManager());
            //container.RegisterType<object, PatientContractsView>(viewNameResolver.Resolve<PatientContractsViewModel>(), new ContainerControlledLifetimeManager());
            //container.RegisterType<object, PersonDocumentsView>(viewNameResolver.Resolve<PersonDocumentsViewModel>(), new ContainerControlledLifetimeManager());
            //regionManager.RegisterViewWithRegion(RegionNames.ModuleList, typeof(InfoHeaderView));
            //regionManager.RegisterViewWithRegion(RegionNames.ModuleList, typeof(DocumentsHeaderView));
            //regionManager.RegisterViewWithRegion(RegionNames.ModuleList, typeof(ContractsHeaderView));
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/ScheduleModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
        }

        private void InitiateLongRunningOperations()
        {
        }
    }
}
