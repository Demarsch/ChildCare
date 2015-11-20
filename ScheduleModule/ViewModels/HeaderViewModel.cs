using System;
using Core.Data;
using Core.Wpf.Events;
using Core.Wpf.Services;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using ScheduleModule.Misc;
using Shell.Shared;

namespace ScheduleModule.ViewModels
{
    public class HeaderViewModel : BindableBase, IActiveAware, IDisposable
    {
        private readonly IRegionManager regionManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IViewNameResolver viewNameResolver;

        public HeaderViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IViewNameResolver viewNameResolver, ContentViewModel contentViewModel)
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
            if (contentViewModel == null)
            {
                throw new ArgumentNullException("contentViewModel");
            }
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.viewNameResolver = viewNameResolver;
            ContentViewModel = contentViewModel;
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
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
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
            var navigationParameters = new NavigationParameters { { ParameterNames.PatientId, patientId  } };
            regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<ContentViewModel>(), navigationParameters);
        }

        public event EventHandler IsActiveChanged = delegate { };

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }
    }
}
