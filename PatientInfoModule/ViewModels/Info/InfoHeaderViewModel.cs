using System;
using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Services;
using PatientInfoModule.Misc;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace PatientInfoModule.ViewModels
{
    public class InfoHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        private readonly IEventAggregator eventAggregator;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        public InfoHeaderViewModel(IEventAggregator eventAggregator,
                                   IRegionManager regionManager,
                                   IViewNameResolver viewNameResolver,
                                   InfoContentViewModel contentViewModel)
        {
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
            if (contentViewModel == null)
            {
                throw new ArgumentNullException("contentViewModel");
            }
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            ContentViewModel = contentViewModel;
            patientId = SpecialValues.NonExistingId;
            SubscribeToEvents();
        }

        public InfoContentViewModel ContentViewModel { get; private set; }

        private int patientId;

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void OnPatientSelected(int patientId)
        {
            this.patientId = patientId;
            LoadSelectedPatientData();
            ActivatePatientInfo();
        }

        private void LoadSelectedPatientData()
        {
            //TODO:
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        private void ActivatePatientInfo()
        {
            if (patientId == SpecialValues.NonExistingId)
            {
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<EmptyPatientInfoViewModel>());
            }
            else
            {
                var navigationParameters = new NavigationParameters { { ParameterNames.PatientId, patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<InfoContentViewModel>(), navigationParameters);
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
                        ActivatePatientInfo();
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}