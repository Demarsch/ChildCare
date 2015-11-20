using System;
using System.Windows;
using Core.Wpf.Services;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using ScheduleModule.Services;
using ScheduleModule.ViewModels;
using ScheduleModule.Views;
using Shell.Shared;

namespace ScheduleModule
{
    [Module(ModuleName = WellKnownModuleNames.ScheduleModule)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

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
            this.container = container;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }

        public void Initialize()
        {
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
        }

        private void RegisterViewModels()
        {
            container.RegisterType<ScheduleAssignmentUpdateViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<TimeTickerViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<ContentViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            container.RegisterType<object, ContentView>(viewNameResolver.Resolve<ContentViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, typeof(HeaderView));
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/ScheduleModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IScheduleService, ScheduleService>(new ContainerControlledLifetimeManager());
        }
    }
}
