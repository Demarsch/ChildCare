using System;
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
using Prism.Commands;
using Shell.Shared;

namespace PatientInfoModule.ViewModels
{
    public class ContractsHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        private readonly IDbContextProvider contextProvider;
        private readonly ILog log;
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;
        private readonly IViewNameResolver viewNameResolver;
        private readonly PatientContractsViewModel contractsViewModel;

        public ContractsHeaderViewModel(IDbContextProvider contextProvider, ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver, PatientContractsViewModel contractsViewModel)
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
            this.contractsViewModel = contractsViewModel;
            patientId = SpecialId.NonExisting;
            SubscribeToEvents();
        }       

        private int patientId;
        public ICommand AddContractCommand { get { return contractsViewModel.AddContractCommand; } }
        public ICommand SaveContractCommand { get { return contractsViewModel.SaveContractCommand; } }
        public ICommand RemoveContractCommand { get { return contractsViewModel.RemoveContractCommand; } }
        public ICommand PrintContractCommand { get { return contractsViewModel.PrintContractCommand; } }
        public ICommand PrintAppendixCommand { get { return contractsViewModel.PrintAppendixCommand; } }
        public ICommand AddRecordCommand { get { return contractsViewModel.AddRecordCommand; } }
        public ICommand RemoveRecordCommand { get { return contractsViewModel.RemoveRecordCommand; } }
        public ICommand AddAppendixCommand { get { return contractsViewModel.AddAppendixCommand; } }
        public ICommand RemoveAppendixCommand { get { return contractsViewModel.RemoveAppendixCommand; } }

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void OnPatientSelected(int patientId)
        {
            this.patientId = patientId;
            LoadSelectedPatientContracts();
            ActivatePatientContracts();
        }

        private void LoadSelectedPatientContracts()
        {
            //TODO:
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        private void ActivatePatientContracts()
        {
            if (patientId == SpecialId.NonExisting)
            {
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<EmptyPatientInfoViewModel>());
            }
            else
            {
                var navigationParameters = new NavigationParameters { { "PatientId", patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PatientContractsViewModel>(), navigationParameters);
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
                        ActivatePatientContracts();
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
