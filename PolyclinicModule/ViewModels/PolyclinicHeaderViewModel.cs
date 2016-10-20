using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Extensions;
using Core.Wpf.Services;
using Core.Extensions;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shared.PatientRecords.Events;
using Shared.PatientRecords.Misc;
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
    public class PolyclinicHeaderViewModel : BindableBase, IActiveAware, IDisposable
    {
        #region Fields       

        private readonly Func<PersonRecordsToolboxViewModel> personRecordsToolboxViewModelFactory;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors
        public PolyclinicHeaderViewModel(IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver, 
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
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.eventAggregator = eventAggregator;

            this.personRecordsToolboxViewModelFactory = personRecordsToolboxViewModelFactory;
            PersonRecordsToolboxViewModel = personRecordsToolboxViewModel;

            SubscribeToEvents();
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

        private int patientId;

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<PolyclinicPersonListChangedEvent>().Subscribe(OnPolyclinicPersonListItemSelected);
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<PolyclinicPersonListChangedEvent>().Unsubscribe(OnPolyclinicPersonListItemSelected);
        }

        private void OnPolyclinicPersonListItemSelected(object parameters)
        {
            if (parameters == null) return;
            int personId = (parameters as object[])[0].ToInt();
            int assignmentId = (parameters as object[])[1].ToInt();
            int recordId = (parameters as object[])[2].ToInt();

            this.patientId = personId;     
        }      

        private void ActivateHeader()
        {            
            if (personRecordsToolboxViewModel == null)
                PersonRecordsToolboxViewModel = personRecordsToolboxViewModelFactory();
            PersonRecordsToolboxViewModel.ActivatePersonRecords();

            regionManager.RequestNavigate(RegionNames.ListItems, viewNameResolver.Resolve<PolyclinicPersonListViewModel>());

            if (SpecialValues.IsNewOrNonExisting(patientId))
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PolyclinicEmptyViewModel>());
            else
            {
                var navigationParameters = new NavigationParameters { { ParameterNames.PatientId, patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PersonRecordsViewModel>(), navigationParameters);
            }
        }        

        #region Events
        public event EventHandler IsActiveChanged = delegate { };
        #endregion
    }
}


          