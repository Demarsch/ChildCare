using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Data;
using log4net;
using Prism.Mvvm;
using Prism.Commands;
using Prism.Interactivity;
using Prism.Events;
using Prism.Common;
using Prism.Regions;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using Shell.Shared;
using Prism;

namespace AdminModule.ViewModels
{
    public class AdminHeaderViewModel : BindableBase, IActiveAware
    {
        ILog log;
        IEventAggregator eventAggregator;
        IRegionManager regionManager;
        IViewNameResolver viewNameResolver;

        public AdminHeaderViewModel(ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver)
        {
            this.log = log;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }

        private void PerformNavigation(string navSource = null)
        {
            if (string.IsNullOrWhiteSpace(navSource))
            {
                if (regionManager.Regions[RegionNames.ModuleContent].ActiveViews.Any())
                    return;
                navSource = viewNameResolver.Resolve<AdminEmptyViewModel>();
            }
            regionManager.RequestNavigate(RegionNames.ModuleContent, navSource);
        }

        private bool isActive;
        public bool IsActive
        { 
            get { return isActive; }
            set
            {
                SetProperty(ref isActive, value);
                if (value)
                    PerformNavigation();
            }
        }

        public event EventHandler IsActiveChanged = (sender, e) => { };

        private DelegateCommand reportTemplatesManagerCommand;
        public DelegateCommand ReportTemplateManagerCommand
        {
            get
            {
                return reportTemplatesManagerCommand ?? (reportTemplatesManagerCommand = new DelegateCommand(() =>
                {
                    PerformNavigation(viewNameResolver.Resolve<ReportTemplatesManagerViewModel>());
                }));
            }
        }

    }
}
