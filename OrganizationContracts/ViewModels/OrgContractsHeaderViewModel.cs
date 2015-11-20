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

namespace OrganizationContractsModule.ViewModels
{
    public class OrgContractsHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        private readonly ILog log;
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;
        private readonly IViewNameResolver viewNameResolver;
        private readonly OrgContractsViewModel contractsViewModel;

        public OrgContractsHeaderViewModel(ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver, OrgContractsViewModel contractsViewModel)
        {            
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
            this.log = log;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.contractsViewModel = contractsViewModel;
        }

        public ICommand AddContractCommand { get { return contractsViewModel.AddContractCommand; } }
        public ICommand SaveContractCommand { get { return contractsViewModel.SaveContractCommand; } }
        public ICommand RemoveContractCommand { get { return contractsViewModel.RemoveContractCommand; } }
        public ICommand AddOrganizationCommand { get { return contractsViewModel.AddOrganizationCommand; } }

        public ICommand AttachDocumentCommand { get { return contractsViewModel.AttachDocumentCommand; } }
        public ICommand DetachDocumentCommand { get { return contractsViewModel.DetachDocumentCommand; } }
        public ICommand AttachDICOMCommand { get { return contractsViewModel.AttachDICOMCommand; } }
        public ICommand DetachDICOMCommand { get { return contractsViewModel.DetachDICOMCommand; } }

        private bool allowDocuments;
        public bool AllowDocuments 
        { 
            get { return contractsViewModel.AllowDocuments; }
            set { SetProperty(ref allowDocuments, value); }
        }

        private bool allowDICOM;
        public bool AllowDICOM
        {
            get { return contractsViewModel.AllowDocuments; }
            set { SetProperty(ref allowDICOM, value); }
        }

        private bool canAttachDICOM;
        public bool CanAttachDICOM
        {
            get { return contractsViewModel.CanAttachDICOM; }
            set { SetProperty(ref canAttachDICOM, value); }
        }

        private bool canDetachDICOM;
        public bool CanDetachDICOM
        {
            get { return !contractsViewModel.CanAttachDICOM; }
            set { SetProperty(ref canDetachDICOM, value); }
        }

        public void Dispose()
        {
            
        }
       
        private void ActivateOrganizationContracts()
        {
            regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<OrgContractsViewModel>());
            AllowDocuments = contractsViewModel.AllowDocuments;
            AllowDICOM = contractsViewModel.AllowDICOM;
            CanAttachDICOM = contractsViewModel.CanAttachDICOM;
            CanDetachDICOM = !contractsViewModel.CanAttachDICOM;
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
                        ActivateOrganizationContracts();
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
