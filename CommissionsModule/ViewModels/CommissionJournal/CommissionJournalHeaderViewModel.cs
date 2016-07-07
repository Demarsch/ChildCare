using Core.Data.Misc;
using Core.Wpf.Extensions;
using Core.Wpf.Services;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Shared.Commissions.ViewModels;
using Shell.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CommissionsModule.ViewModels
{
    public class CommissionJournalHeaderViewModel : BindableBase, IActiveAware
    {
        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly Func<AssignmentCommissionViewModel> assignmentCommissionViewModelFactory;

        public CommissionJournalHeaderViewModel(IRegionManager regionManager,
                                             IViewNameResolver viewNameResolver,
                                             CommissionJournalViewModel commissionJournalViewModel,
                                             Func<AssignmentCommissionViewModel> assignmentCommissionViewModelFactory)
        {
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (commissionJournalViewModel == null)
            {
                throw new ArgumentNullException("commissionJournalViewModel");
            }
            if (assignmentCommissionViewModelFactory == null)
            {
                throw new ArgumentNullException("assignmentCommissionViewModelFactory");
            }
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.assignmentCommissionViewModelFactory = assignmentCommissionViewModelFactory;
            CommissionJournalViewModel = commissionJournalViewModel;
        }

        public CommissionJournalViewModel CommissionJournalViewModel { get; private set; }

        private AssignmentCommissionViewModel assignmentCommissionViewModel;
        public AssignmentCommissionViewModel AssignmentCommissionViewModel
        {
            get { return assignmentCommissionViewModel; }
            set { SetProperty(ref assignmentCommissionViewModel, value); }
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
                        ActivateContent();
                    }
                }
            }
        }

        private void ActivateContent()
        {
            regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<CommissionJournalViewModel>());
            if (assignmentCommissionViewModel == null)
                AssignmentCommissionViewModel = assignmentCommissionViewModelFactory();
            AssignmentCommissionViewModel.PersonId = SpecialValues.NonExistingId;
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
