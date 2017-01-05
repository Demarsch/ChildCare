using System;
using System.Linq;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Wpf.Events;
using Core.Wpf.Services;
using log4net;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Shell.Shared;

namespace StatisticsModule.ViewModels
{
    public class StatisticsHeaderViewModel : BindableBase, IActiveAware
    {
        private readonly ILog log;
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;
        private readonly IViewNameResolver viewNameResolver;

        public StatisticsHeaderViewModel(ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver)
        {            
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            this.log = log;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;

            activeSubViewName = viewNameResolver.Resolve<StatisticsEmptyViewModel>();
            RecordsStatisticsCommand = new DelegateCommand(() => NavigateToSubView(viewNameResolver.Resolve<RecordsStatisticsViewModel>()));
            ScheduleStatisticsCommand = new DelegateCommand(() => NavigateToSubView(viewNameResolver.Resolve<ScheduleStatisticsViewModel>()));
            RoomCapacityStatisticsCommand = new DelegateCommand(() => NavigateToSubView(viewNameResolver.Resolve<RoomCapacityStatisticsViewModel>()));
        }

        private string activeSubViewName;


        private void NavigateToSubView(string newActiveSubViewName)
        {
            regionManager.RequestNavigate(RegionNames.ModuleContent, newActiveSubViewName);
            activeSubViewName = newActiveSubViewName;
        }

        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (SetProperty(ref isActive, value))
                {
                    OnIsActiveChanged();
                }
                if (value)
                {
                    NavigateToSubView(activeSubViewName);
                }
            }
        }

        public event EventHandler IsActiveChanged;

        protected virtual void OnIsActiveChanged()
        {
            var handler = IsActiveChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public ICommand RecordsStatisticsCommand { get; private set; }
        public ICommand ScheduleStatisticsCommand { get; private set; }
        public ICommand RoomCapacityStatisticsCommand { get; private set; }
    }
}
