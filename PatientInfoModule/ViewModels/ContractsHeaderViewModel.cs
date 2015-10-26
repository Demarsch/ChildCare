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

        public ContractsHeaderViewModel(IDbContextProvider contextProvider, ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver)
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
            patientId = SpecialId.NonExisting;

            this.AddContractCommand = new DelegateCommand(AddContract);
            this.SaveContractCommand = new DelegateCommand(SaveContract);
            this.RemoveContractCommand = new DelegateCommand(RemoveContract);
            this.PrintContractCommand = new DelegateCommand(PrintContract);
            this.PrintAppendixCommand = new DelegateCommand(PrintAppendix);
            this.AddRecordCommand = new DelegateCommand(AddRecord);
            this.RemoveRecordCommand = new DelegateCommand(RemoveRecord);
            this.AddAppendixCommand = new DelegateCommand(AddAppendix);
            this.RemoveAppendixCommand = new DelegateCommand(RemoveAppendix);

            SubscribeToEvents();
        }

        private void RemoveAppendix()
        {
            throw new NotImplementedException();
        }

        private void AddAppendix()
        {
            throw new NotImplementedException();
        }

        private void RemoveRecord()
        {
            throw new NotImplementedException();
        }

        private void AddRecord()
        {
            throw new NotImplementedException();
        }

        private void PrintAppendix()
        {
            throw new NotImplementedException();
        }

        private void PrintContract()
        {
            throw new NotImplementedException();
        }

        private void RemoveContract()
        {
            throw new NotImplementedException();
        }

        private void SaveContract()
        {
            throw new NotImplementedException();
        }

        private void AddContract()
        {
            throw new NotImplementedException();
        }

        private int patientId;
        public ICommand AddContractCommand { get; private set; }
        public ICommand SaveContractCommand { get; private set; }
        public ICommand RemoveContractCommand { get; private set; }
        public ICommand PrintContractCommand { get; private set; }
        public ICommand PrintAppendixCommand { get; private set; }
        public ICommand AddRecordCommand { get; private set; }
        public ICommand RemoveRecordCommand { get; private set; }
        public ICommand AddAppendixCommand { get; private set; }
        public ICommand RemoveAppendixCommand { get; private set; }

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
