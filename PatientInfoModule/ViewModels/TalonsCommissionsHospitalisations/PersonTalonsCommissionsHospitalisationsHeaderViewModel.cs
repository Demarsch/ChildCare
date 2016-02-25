using System;
using System.Linq;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Wpf.Events;
using Core.Wpf.Services;
using log4net;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace PatientInfoModule.ViewModels
{
    public class PersonTalonsCommissionsHospitalisationsHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        private readonly IDbContextProvider contextProvider;
        private readonly ILog log;
        private readonly IEventAggregator eventAggregator;        
        private readonly IRegionManager regionManager;
        private readonly IViewNameResolver viewNameResolver;
        private readonly PersonTalonsCommissionsHospitalisationsViewModel viewModel;

        public PersonTalonsCommissionsHospitalisationsHeaderViewModel(IDbContextProvider contextProvider, ILog log, IEventAggregator eventAggregator, 
               IRegionManager regionManager, IViewNameResolver viewNameResolver, PersonTalonsCommissionsHospitalisationsViewModel viewModel)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
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
            this.contextProvider = contextProvider;
            this.log = log;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.viewModel = viewModel;
            patientId = SpecialValues.NonExistingId;
            SubscribeToEvents();
        }

        private int patientId;
        public ICommand EditTalonCommand { get { return viewModel.EditTalonCommand; } }
        public ICommand RemoveTalonCommand { get { return viewModel.RemoveTalonCommand; } }
        public ICommand LinkTalonToHospitalisationCommand { get { return viewModel.LinkTalonToHospitalisationCommand; } }

        public ICommand RemoveCommissionProtocolCommand { get { return viewModel.RemoveCommissionProtocolCommand; } }
        public ICommand PrintCommissionProtocolCommand { get { return viewModel.PrintCommissionProtocolCommand; } }

        private int selectedTalonId;
        public int SelectedTalonId
        {
            get { return selectedTalonId; }
            set { SetProperty(ref selectedTalonId, value); }
        }

        private int selectedCommissionId;
        public int SelectedCommissionId
        {
            get { return selectedCommissionId; }
            set { SetProperty(ref selectedCommissionId, value); }
        }

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Subscribe(OnPatientSelected);
            eventAggregator.GetEvent<PubSubEvent<PersonTalonViewModel>>().Subscribe(OnTalonSelected);
            eventAggregator.GetEvent<PubSubEvent<PersonCommissionViewModel>>().Subscribe(OnCommissionSelected);
        }

        private void OnPatientSelected(int patientId)
        {
            this.patientId = patientId;
            ActivateTab();
        }

        private void OnTalonSelected(PersonTalonViewModel talon)
        {
            if (talon != null && !SpecialValues.IsNewOrNonExisting(talon.Id))
                SelectedTalonId = talon.Id;
        }

        private void OnCommissionSelected(PersonCommissionViewModel commission)
        {
            if (commission != null && !SpecialValues.IsNewOrNonExisting(commission.Id))
                SelectedCommissionId = commission.Id;
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Unsubscribe(OnPatientSelected);
            eventAggregator.GetEvent<PubSubEvent<PersonTalonViewModel>>().Unsubscribe(OnTalonSelected);
            eventAggregator.GetEvent<PubSubEvent<PersonCommissionViewModel>>().Unsubscribe(OnCommissionSelected);
        }

        private void ActivateTab()
        {
            if (patientId == SpecialValues.NonExistingId)
            {
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<EmptyPatientInfoViewModel>());
            }
            else
            {
                var navigationParameters = new NavigationParameters { { "PatientId", patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PersonTalonsCommissionsHospitalisationsViewModel>(), navigationParameters);
            }
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
                        ActivateTab();
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}