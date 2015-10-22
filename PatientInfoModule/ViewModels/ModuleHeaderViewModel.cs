﻿using System;
using System.Linq;
using System.Runtime.Remoting;
using System.Windows;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Wpf.Events;
using Core.Wpf.Services;
using log4net;
using PatientInfoModule.Views;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;

namespace PatientInfoModule.ViewModels
{
    public class ModuleHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ILog log;

        private readonly IEventAggregator eventAggregator;
        
        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private const string PatientIsNotSelected = "не выбран";

        public ModuleHeaderViewModel(IDbContextProvider contextProvider, ILog log, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver)
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
            ShortName = PatientIsNotSelected;
            patientId = SpecialId.NonExisting;
            SubscribeToEvents();
        }

        private int patientId;

        private string shortName;

        public string ShortName
        {
            get { return shortName; }
            set { SetProperty(ref shortName, value); }
        }

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
            LoadSelectedPatientData();
            ActivatePatientInfo();
        }

        private void LoadSelectedPatientData()
        {
            MessageBox.Show("Загрузка данных пациента с Id = " + patientId + " в верхнюю часть риббона");
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        private void ActivatePatientInfo()
        {
            if (patientId == SpecialId.NonExisting)
            {
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<EmptyPatientInfoViewModel>());
            }
            else
            {
                var navigationParameters = new NavigationParameters { { "PatientId", patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PatientInfoViewModel>(), navigationParameters);
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
