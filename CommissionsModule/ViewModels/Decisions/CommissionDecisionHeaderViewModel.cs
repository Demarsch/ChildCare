using Core.Wpf.Extensions;
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
        public CommissionDecisionHeaderViewModel(IRegionManager regionManager, IViewNameResolver viewNameResolver, CommissionDecisionsViewModel commissionDecisionsViewModel)
        {
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (commissionDecisionsViewModel == null)
            {
                throw new ArgumentNullException("commissionDecisionsViewModel");
            }
            CommissionDecisionsViewModel = commissionDecisionsViewModel;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
        }
        #endregion

        #region Properties
        public CommissionDecisionsViewModel CommissionDecisionsViewModel { get; private set; }
        #endregion

        #region IActiveAware implementation

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
                        regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<CommissionDecisionsViewModel>());
                        regionManager.RequestNavigate(RegionNames.ListItems, viewNameResolver.Resolve<CommissionsListViewModel>());
                    }
                    else
                    {
                        regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };

        #endregion
    }
}
