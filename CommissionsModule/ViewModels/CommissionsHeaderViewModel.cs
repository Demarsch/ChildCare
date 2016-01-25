using Core.Wpf.Services;
using log4net;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionsHeaderViewModel: BindableBase, IActiveAware
    {
        private ILog log;
        private IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;
        private readonly IViewNameResolver viewNameResolver;

        public CommissionsHeaderViewModel(ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver)
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
        }

        private void ActivateCommissionsContent()
        {
            regionManager.RequestNavigate(RegionNames.ListItems, viewNameResolver.Resolve<CommissionsListViewModel>());
        }

        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    IsActiveChanged(this, EventArgs.Empty);
                    OnPropertyChanged(() => IsActive);
                    if (value)
                    {
                        ActivateCommissionsContent();
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
