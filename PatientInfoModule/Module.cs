﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Wpf.Events;
using Core.Wpf.Services;
using Fluent;
using log4net;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using PatientInfoModule.ViewModels;
using PatientInfoModule.Views;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;

namespace PatientInfoModule
{
    [Module(ModuleName = WellKnownModuleNames.PatientInfoModule)]
    [ModuleDependency(WellKnownModuleNames.PatientSearchModule)]
    public class Module : IModule
    {
        private const string PatientIsNotSelected = "Пациент не выбран";

        private const string NewPatient = "Новый пациент";

        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly IDbContextProvider contextProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog log;

        public Module(IUnityContainer container,
                      IRegionManager regionManager,
                      IViewNameResolver viewNameResolver,
                      IDbContextProvider contextProvider,
                      IEventAggregator eventAggregator,
                      ILog log)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.container = container;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.contextProvider = contextProvider;
            this.eventAggregator = eventAggregator;
            this.log = log;
        }

        public void Initialize()
        {
            RegisterServices();
            RegisterViews();
        }

        private void RegisterViews()
        {
            //This is required by Prism navigation mechanism to resolve view
            container.RegisterType<object, EmptyPatientInfo>(viewNameResolver.Resolve<EmptyPatientInfoViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterType<object, InfoContent>(viewNameResolver.Resolve<InfoContentViewModel>(), new ContainerControlledLifetimeManager());
            //container.RegisterType<object, PatientContracts>(viewNameResolver.Resolve<PatientContractsViewModel>(), new ContainerControlledLifetimeManager());
            container.RegisterInstance(Common.RibbonGroupName,
                                       new RibbonContextualTabGroup
                                       {
                                           Visibility = Visibility.Visible,
                                           Background = Brushes.SteelBlue,
                                           BorderBrush = Brushes.Blue,
                                           Header = PatientIsNotSelected
                                       });
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelectedAsync);
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<InfoHeader>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<DocumentsHeader>());
            regionManager.RegisterViewWithRegion(RegionNames.ModuleList, () => container.Resolve<ContractsHeader>());
        }

        private async void OnPatientSelectedAsync(int patientId)
        {
            DbContext context = null;
            var ribbonContextualGroup = container.Resolve<RibbonContextualTabGroup>(Common.RibbonGroupName);
            if (patientId == SpecialId.NonExisting)
            {
                ribbonContextualGroup.Header = PatientIsNotSelected;
                return;
            }
            if (patientId == SpecialId.New)
            {
                ribbonContextualGroup.Header = NewPatient;
                return;
            }
            try
            {
                context = contextProvider.CreateNewContext();
                var data = await context.Set<Person>()
                                        .Where(x => x.Id == patientId)
                                        .Select(x => new { x.ShortName, x.GenderId })
                                        .FirstOrDefaultAsync();
                if (data == null)
                {
                    ribbonContextualGroup.Header = PatientIsNotSelected;
                    log.ErrorFormat("Patient with Id {0} not found in database", patientId);
                }
                else
                {
                    ribbonContextualGroup.Header = string.Format("{0} {1}",
                                                                 data.GenderId == Gender.MaleGenderId ? "Пациент" : "Пациентка",
                                                                 data.ShortName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to retrieve patient short name for ribbon contextual group header", ex);
                ribbonContextualGroup.Header = PatientIsNotSelected;
            }
            finally
            {
                if (context != null)
                {
                    context.Dispose();
                }
            }
        }

        private void RegisterServices()
        {
            container.RegisterType<IPatientService, PatientService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRecordService, RecordService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContractService, ContractService>(new ContainerControlledLifetimeManager());
        }
    }
}