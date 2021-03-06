﻿using System;
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
    public class DocumentsHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        private readonly IDbContextProvider contextProvider;
        private readonly ILog log;
        private readonly IEventAggregator eventAggregator;        
        private readonly IRegionManager regionManager;
        private readonly IViewNameResolver viewNameResolver;
        private readonly PersonDocumentsViewModel viewModel;

        public DocumentsHeaderViewModel(IDbContextProvider contextProvider, ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver, PersonDocumentsViewModel viewModel)
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
        public ICommand ScanningCommand { get { return viewModel.ScanningCommand; } }
        public ICommand AddDocumentCommand { get { return viewModel.AddDocumentCommand; } }
        public ICommand RemoveDocumentCommand { get { return viewModel.RemoveDocumentCommand; } }

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
            ActivatePersonDocuments();
        }        

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        private void ActivatePersonDocuments()
        {
            if (patientId == SpecialValues.NonExistingId)
            {
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<EmptyPatientInfoViewModel>());
            }
            else
            {
                var navigationParameters = new NavigationParameters { { "PatientId", patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PersonDocumentsViewModel>(), navigationParameters);
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
                        ActivatePersonDocuments();
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged = delegate { };
    }
}
