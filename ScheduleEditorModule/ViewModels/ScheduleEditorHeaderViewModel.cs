using System;
using System.Linq;
using Core.Data;
using Core.Wpf.Events;
using Core.Wpf.Services;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace ScheduleEditorModule.ViewModels
{
    public class ScheduleEditorHeaderViewModel : BindableBase, IActiveAware
    {
        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        public ScheduleEditorHeaderViewModel(IRegionManager regionManager,
                                             IViewNameResolver viewNameResolver,
                                             ScheduleEditorContentViewModel scheduleEditorContentViewModel)
        {
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (scheduleEditorContentViewModel == null)
            {
                throw new ArgumentNullException("scheduleEditorContentViewModel");
            }
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            ScheduleEditorContentViewModel = scheduleEditorContentViewModel;
        }

        public ScheduleEditorContentViewModel ScheduleEditorContentViewModel { get; private set; }

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
                        ActivateContent();
                    }
                }
            }
        }


        private void ActivateContent()
        {
            regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<ScheduleEditorContentViewModel>());

            var activeListItems = regionManager.Regions[RegionNames.ListItems].ActiveViews.FirstOrDefault();
            if (activeListItems != null)
                regionManager.Regions[RegionNames.ListItems].Deactivate(activeListItems);
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}