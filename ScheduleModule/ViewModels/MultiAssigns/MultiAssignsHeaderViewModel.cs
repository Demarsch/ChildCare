using System;
using System.Linq;
using Core.Data;
using Core.Wpf.Events;
using Core.Wpf.Services;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using ScheduleModule.Misc;
using Shell.Shared;
using Core.Wpf.Misc;

namespace ScheduleModule.ViewModels
{
    public class MultiAssignsHeaderViewModel : BindableBase, IActiveAware, IDisposable
    {
        private readonly IRegionManager regionManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IViewNameResolver viewNameResolver;

        public MultiAssignsHeaderViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IViewNameResolver viewNameResolver, MultiAssignsContentViewModel multiAssignsContentViewModel)
        {
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (multiAssignsContentViewModel == null)
            {
                throw new ArgumentNullException("multiAssignsContentViewModel");
            }
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.viewNameResolver = viewNameResolver;
            MultiAssignsContentViewModel = multiAssignsContentViewModel;
            SubscribeToEvents();
        }

        private int patientId;

        private void OnPatientSelected(int patientId)
        {
            this.patientId = patientId;
            ActivateContent();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        public MultiAssignsContentViewModel MultiAssignsContentViewModel { get; private set; }

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
            var navigationParameters = new NavigationParameters { { ParameterNames.PatientId, patientId  } };
            regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<MultiAssignsContentViewModel>(), navigationParameters);
        }

        public event EventHandler IsActiveChanged = delegate { };

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }
    }
}
