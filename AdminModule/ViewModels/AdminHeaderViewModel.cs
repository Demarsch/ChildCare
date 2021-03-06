﻿using System;
using System.Linq;
using System.Windows.Input;
using Core.Wpf.Services;
using log4net;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace AdminModule.ViewModels
{
    public class AdminHeaderViewModel : BindableBase, IActiveAware
    {
        private ILog log;

        private IEventAggregator eventAggregator;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        public AdminHeaderViewModel(ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver)
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
            activeSubViewName = viewNameResolver.Resolve<AdminEmptyViewModel>();
            GoToReportTemplateManagerCommand = new DelegateCommand(() => NavigateToSubView(viewNameResolver.Resolve<ReportTemplatesManagerViewModel>()));
            GoToUserAccessManagerCommand = new DelegateCommand(() => NavigateToSubView(viewNameResolver.Resolve<UserAccessManagerViewModel>()));
            GoToDatabaseValidationCommand = new DelegateCommand(() => NavigateToSubView(viewNameResolver.Resolve<DatabaseValidationViewModel>()));
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

        public ICommand GoToReportTemplateManagerCommand { get; private set; }

        public ICommand GoToUserAccessManagerCommand { get; private set; }

        public ICommand GoToDatabaseValidationCommand { get; private set; }
    }
}