using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Prism.Mvvm;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
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
