using System;
using System.Windows;
using Core.Data;
using Core.Wpf.Services;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using ScheduleEditorModule.Services;
using ScheduleEditorModule.ViewModels;
using ScheduleEditorModule.Views;
using Shell.Shared;

namespace ScheduleEditorModule
{
    [Module(ModuleName = WellKnownModuleNames.ScheduleEditorModule, OnDemand = true)]
    [PermissionRequired(Permission.ScheduleEditorModuleAccess)]
    public class Module : IModule
    {
        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly ILog log;

        public Module(IUnityContainer container, IRegionManager regionManager, IViewNameResolver viewNameResolver, ILog log)
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
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.container = container;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.log = log;
        }

        public void Initialize()
        {
            log.InfoFormat("{0} module init start", WellKnownModuleNames.ScheduleEditorModule);
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("{0} module init finished", WellKnownModuleNames.ScheduleEditorModule);
        }

        private void RegisterViewModels()
        {
            container.RegisterType<ScheduleEditorContentViewModel>(new ContainerControlledLifetimeManager());
            container.RegisterType<ScheduleEditorEditDayViewModel>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            container.RegisterType<object, ScheduleEditorContentView>(viewNameResolver.Resolve<ScheduleEditorContentViewModel>(), new ContainerControlledLifetimeManager());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, typeof(ScheduleEditorHeaderView));
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/ScheduleEditorModule;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            container.RegisterType<IScheduleEditorService, ScheduleEditorService>(new ContainerControlledLifetimeManager());
        }
    }
}
