using System;
using Core.Wpf.Services;
using Prism;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace ScheduleModule.ViewModels
{
    public class HeaderViewModel : BindableBase, IActiveAware
    {
        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        public HeaderViewModel(IRegionManager regionManager, IViewNameResolver viewNameResolver, ContentViewModel contentViewModel)
        {
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (contentViewModel == null)
            {
                throw new ArgumentNullException("contentViewModel");
            }
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            ContentViewModel = contentViewModel;
        }

        public ContentViewModel ContentViewModel { get; private set; }

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
            regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<ContentViewModel>());
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
