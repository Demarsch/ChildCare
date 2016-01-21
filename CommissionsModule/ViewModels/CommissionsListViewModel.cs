﻿using CommissionsModule.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Regions;

namespace CommissionsModule.ViewModels
{
    public class CommissionsListViewModel : BindableBase, INavigationAware
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly IDialogServiceAsync dialogService;
        private readonly IDialogService messageService;
        private readonly ILog logService;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private CancellationTokenSource currentOperationToken;
        #endregion

        #region  Constructors
        public CommissionsListViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IDialogService messageService, 
                                        IUserService userService, IEventAggregator eventAggregator)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }           
            this.dialogService = dialogService;
            this.userService = userService;
            this.messageService = messageService;
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;

            Filters = new ObservableCollectionEx<FieldValue>();
            Commissions = new ObservableCollectionEx<CommissionProtocolViewModel>();
            BusyMediator = new BusyMediator();                        
        }
        #endregion

        #region Properties
        public BusyMediator BusyMediator { get; set; }

        public ObservableCollectionEx<FieldValue> Filters { get; set; }

        private int selectedFilter;
        public int SelectedFilter
        {
            get { return selectedFilter; }
            set 
            {
                if (SetProperty(ref selectedFilter, value))
                {
                    ShowDateFilter = commissionService.IsCommissionFilterHasDate(value);
                    if (!ShowDateFilter)
                        FilterDate = (DateTime?)null;
                    LoadCommissionProtocolsAsync();
                }
            }
        }

        private DateTime? filterDate;
        public DateTime? FilterDate
        {
            get { return filterDate; }
            set
            {
                if (SetProperty(ref filterDate, value))
                    LoadCommissionProtocolsAsync();
            }
        }

        private bool showDateFilter;
        public bool ShowDateFilter
        {
            get { return showDateFilter; }
            set { SetProperty(ref showDateFilter, value); }
        }

        private bool onlyMyCommissions;
        public bool OnlyMyCommissions
        {
            get { return onlyMyCommissions; }
            set
            {
                if (SetProperty(ref onlyMyCommissions, value))
                    LoadCommissionProtocolsAsync();
            }
        }

        public ObservableCollectionEx<CommissionProtocolViewModel> Commissions { get; set; }

        private CommissionProtocolViewModel selectedCommission;
        public CommissionProtocolViewModel SelectedCommission
        {
            get { return selectedCommission; }
            set { SetProperty(ref selectedCommission, value); }
        }

        #endregion

        #region Methods

        public void LoadDataSources()
        {
            Filters.Clear();            
            try
            {
                Filters.AddRange(commissionService.GetCommissionFilters().Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                if (Filters.Any())
                    SelectedFilter = Filters.First().Value;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission filters");
                messageService.ShowError("Не удалось загрузить фильтры.");
            }
        }

        public async void LoadCommissionProtocolsAsync()
        {
            Commissions.Clear();
            BusyMediator.Activate("Загрузка протоколов...");
            logService.Info("Loading commission protocols...");
            IDisposableQueryable<CommissionProtocol> commissionProtocolsQuery = null;
            try
            {
                commissionProtocolsQuery = commissionService.GetCommissionProtocols(SelectedFilter, FilterDate, OnlyMyCommissions);

                //TODO: PersonTalon can be null
                var commissionProtocolsSelectQuery = await commissionProtocolsQuery.Select(x => new
                    {
                        Id = x.Id,
                        PatientFIO = x.Person.ShortName,
                        BirthDate = x.Person.BirthDate.Year,
                        TalonNumber = x.PersonTalon.TalonNumber,
                        TalonMKB = x.PersonTalon.MKB,
                        TalonDate = x.PersonTalon.TalonDateTime,
                        MKB = x.MKB,
                        IncomeDateTime = x.IncomeDateTime,
                        IsCompleted = x.IsCompleted
                    }).ToArrayAsync();

                var result = commissionProtocolsSelectQuery.Select(x => new CommissionProtocolViewModel()
                    {
                        Id = x.Id,
                        PatientFIO = x.PatientFIO,
                        BirthDate = x.BirthDate + " г.р.",
                        Talon = "(" + x.TalonNumber + (!string.IsNullOrWhiteSpace(x.TalonMKB) ? " - " + x.TalonMKB : string.Empty) + ") от " + x.TalonDate.ToShortDateString(),                       
                        MKB = x.MKB,
                        IncomeDateTime = x.IncomeDateTime.ToShortDateString(),
                        IsCompleted = x.IsCompleted
                    }).ToArray();

                Commissions.AddRange(result);
                if (Commissions.Any())
                    SelectedCommission = Commissions.First();
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission protocols");
                messageService.ShowError("Не удалось загрузить протоколы комиссий. ");
            }
            finally
            {
                if (commissionProtocolsQuery != null)
                    commissionProtocolsQuery.Dispose();
                BusyMediator.Deactivate();
            }
        }

        #endregion


        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadDataSources();
        }
    }
}