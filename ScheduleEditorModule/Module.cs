using System;
using System.Windows;
using Core.Wpf.Services;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using ScheduleEditorModule.Services;
using ScheduleEditorModule.ViewModels;
using ScheduleEditorModule.Views;
using Shell.Shared;

namespace ScheduleEditorModule
{
    [Module(ModuleName = WellKnownModuleNames.ScheduleEditor)]
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
            container.RegisterType<ContentViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<ScheduleEditorEditDayViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            container.RegisterType<object, ContentView>(viewNameResolver.Resolve<ContentViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, typeof(HeaderView));
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/ScheduleEditorModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IScheduleEditorService, ScheduleEditorService>(new ContainerControlledLifetimeManager());
        }
    }
}
