using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Extensions;
using Core.Wpf.Services;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shared.PatientRecords.ViewModels;
using Shell.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PolyclinicModule.ViewModels
{
    public class PolyclinicHeaderViewModel : BindableBase, IActiveAware
    {
        #region Fields       

        private readonly Func<PersonRecordsToolboxViewModel> personRecordsToolboxViewModelFactory;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        #endregion

        #region Constructors
        public PolyclinicHeaderViewModel(IRegionManager regionManager, IViewNameResolver viewNameResolver, 
                                         PersonRecordsToolboxViewModel personRecordsToolboxViewModel, Func<PersonRecordsToolboxViewModel> personRecordsToolboxViewModelFactory)
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

            this.personRecordsToolboxViewModelFactory = personRecordsToolboxViewModelFactory;
            PersonRecordsToolboxViewModel = personRecordsToolboxViewModel;           
        }

        #endregion

        private PersonRecordsToolboxViewModel personRecordsToolboxViewModel;
        public PersonRecordsToolboxViewModel PersonRecordsToolboxViewModel
        {
            get { return personRecordsToolboxViewModel; }
            set { SetProperty(ref personRecordsToolboxViewModel, value); }
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
                        ActivateHeader();
                    }
                    else
                    {
                        regionManager.Regions[RegionNames.ListItems].DeactivateActiveViews();
                    }
                }
            }
        }

        private void ActivateHeader()
        {            
            if (personRecordsToolboxViewModel == null)
                PersonRecordsToolboxViewModel = personRecordsToolboxViewModelFactory();
            PersonRecordsToolboxViewModel.ActivatePersonRecords();

            regionManager.RequestNavigate(RegionNames.ListItems, viewNameResolver.Resolve<PolyclinicPersonListViewModel>());
        }        

        #region Events
        public event EventHandler IsActiveChanged = delegate { };
        #endregion
    }
}


          