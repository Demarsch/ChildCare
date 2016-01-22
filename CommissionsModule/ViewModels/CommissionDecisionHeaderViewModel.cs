using Core.Wpf.Services;
using Prism;
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
    public class CommissionDecisionHeaderViewModel : BindableBase, IActiveAware
    {
        #region Fields
        private readonly IRegionManager regionManager;
        private readonly IViewNameResolver viewNameResolver;
        #endregion

        #region Constructors
        public CommissionDecisionHeaderViewModel(IRegionManager regionManager, IViewNameResolver viewNameResolver)
        {
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }
        #endregion

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
                        regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<CommissionDecisionViewModel>());
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
