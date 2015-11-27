using System;
using Core.Data;
using Core.Wpf.Events;
using Core.Wpf.Services;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace ScheduleEditorModule.ViewModels
{
    public class ScheduleEditorHeaderViewModel : BindableBase, IActiveAware, IDisposable
    {
        private readonly IRegionManager regionManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IViewNameResolver viewNameResolver;

        public ScheduleEditorHeaderViewModel(IRegionManager regionManager,
                                             IEventAggregator eventAggregator,
                                             IViewNameResolver viewNameResolver,
                                             ScheduleEditorContentViewModel scheduleEditorContentViewModel)
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
            if (scheduleEditorContentViewModel == null)
            {
                throw new ArgumentNullException("scheduleEditorContentViewModel");
            }
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.viewNameResolver = viewNameResolver;
            ScheduleEditorContentViewModel = scheduleEditorContentViewModel;
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

        public ScheduleEditorContentViewModel ScheduleEditorContentViewModel { get; private set; }

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
            regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<ScheduleEditorContentViewModel>());
        }

        public event EventHandler IsActiveChanged = delegate { };

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }
    }
}