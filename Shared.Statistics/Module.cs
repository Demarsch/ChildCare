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
using Core.Data;
using System.Data.Entity;
using Core.Wpf.Events;
using System.Windows;

namespace Statistics
{
    public class Module
    {
        #region Fields

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
            log.InfoFormat("Statistics library start");
            RegisterServices();
            RegisterViewModels();
            RegisterViews();
            log.InfoFormat("Statistics library finished");
        }

        private void RegisterViewModels()
        {
            //container.RegisterType<PersonRecordsViewModel>(new ContainerControlledLifetimeManager());
            //container.RegisterType<PersonRecordEditorViewModel>(new TransientLifetimeManager());           
        }

        private void RegisterViews()
        {
            //container.RegisterType<object, PersonRecordsView>(viewNameResolver.Resolve<PersonRecordsViewModel>(), new ContainerControlledLifetimeManager());       

            //regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<PersonRecordsHeader>());
            //regionManager.RegisterViewWithRegion(RegionNames.ModuleContent, () => container.Resolve<PersonRecordsView>());
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/Shared.Statistics;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }

        private void RegisterServices()
        {
            //container.RegisterType<IHierarchicalRepository, HierarchicalRepository>(new ContainerControlledLifetimeManager());
        }
        #endregion

    }
}

