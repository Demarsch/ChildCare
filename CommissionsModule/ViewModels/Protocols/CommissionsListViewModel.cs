using CommissionsModule.Services;
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
using System.Windows.Media;
using Core.Notification;

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
        private readonly INotificationService notificationService;
        private readonly IEventAggregator eventAggregator;
        private TaskCompletionSource<bool> completionTaskSource;
        #endregion

        #region  Constructors
        public CommissionsListViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IDialogService messageService, 
                                        IUserService userService, INotificationService notificationService, IEventAggregator eventAggregator)
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
            if (notificationService == null)
            {
                throw new ArgumentNullException("notificationService");
            }
            this.dialogService = dialogService;
            this.userService = userService;
            this.messageService = messageService;
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            this.notificationService = notificationService;

            Filters = new ObservableCollectionEx<FieldValue>();
            Commissions = new ObservableCollectionEx<CommissionProtocolViewModel>();
            BusyMediator = new BusyMediator();

            LoadDataSources();
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
                    if (ShowDateFilter)
                        FilterDate = DateTime.Now;
                    else
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
            set 
            { 
                if (SetProperty(ref selectedCommission, value))
                {
                    eventAggregator.GetEvent<PubSubEvent<int>>().Publish(value.Id);
                }
            }
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
            BusyMediator.Activate("Загрузка протоколов...");
            logService.Info("Loading commission protocols...");
            Commissions.Clear();
            UnsubscribeToCommissionsProtocolsChanges();
            IDisposableQueryable<CommissionProtocol> commissionProtocolsQuery = null;
            try
            {
                commissionProtocolsQuery = commissionService.GetCommissionProtocols(SelectedFilter, FilterDate, OnlyMyCommissions);

                var commissionProtocolsSelectQuery = await commissionProtocolsQuery.Select(x => new
                    {
                        Id = x.Id,
                        PatientFIO = x.Person.ShortName,
                        BirthDate = x.Person.BirthDate.Year,
                        Talon = x.PersonTalon != null ? new { TalonNumber = x.PersonTalon.TalonNumber, TalonDate = x.PersonTalon.TalonDateTime } : null,
                        MKB = x.MKB,
                        IncomeDateTime = x.IncomeDateTime,
                        IsCompleted = x.IsCompleted,
                        DecisionId = x.DecisionId
                    }).ToArrayAsync();

                var result = commissionProtocolsSelectQuery.Select(x => new CommissionProtocolViewModel()
                    {
                        Id = x.Id,
                        PatientFIO = x.PatientFIO,
                        BirthDate = x.BirthDate + " г.р.",
                        Talon = x.Talon != null ? x.Talon.TalonNumber + " от " + x.Talon.TalonDate.ToShortDateString() : "талон отсутствует",                       
                        MKB = "МКБ: " + x.MKB,
                        IncomeDateTime = x.IncomeDateTime.ToShortDateString(),
                        IsCompleted = x.IsCompleted,
                        DecisionColorHex = commissionService.GetDecisionColorHex(x.DecisionId)
                    }).ToArray();

                Commissions.AddRange(result);
                await SubscribeToCommissionsProtocolsChanges();
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

        private INotificationServiceSubscription<CommissionProtocol> comissionsProtocolsChangeSubscription;

        private async Task<bool> SubscribeToCommissionsProtocolsChanges()
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();
            comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>();
            if (comissionsProtocolsChangeSubscription != null)
                comissionsProtocolsChangeSubscription.Notified += OnCommissionProtocolNotificationRecievedAsync;
            completionTaskSource.SetResult(true);
            return true;
        }

        private void UnsubscribeToCommissionsProtocolsChanges()
        {
            if (comissionsProtocolsChangeSubscription != null)
            {
                comissionsProtocolsChangeSubscription.Notified -= OnCommissionProtocolNotificationRecievedAsync;
                comissionsProtocolsChangeSubscription.Dispose();
            }
        }

        private async void OnCommissionProtocolNotificationRecievedAsync(object sender, NotificationEventArgs<CommissionProtocol> e)
        {
            await UpdateCommissionProtocolAsync((e.OldItem ?? e.NewItem).Id);
        }

        private async Task<bool> UpdateCommissionProtocolAsync(int protocolId)
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();
            var commissionProtocol = commissionService.GetCommissionProtocolById(protocolId).First();
            SelectedCommission.BirthDate = commissionProtocol.Person.BirthDate + " г.р.";
            SelectedCommission.Talon = commissionProtocol.PersonTalon != null ? commissionProtocol.PersonTalon.TalonNumber + " от " + commissionProtocol.PersonTalon.TalonDateTime.ToShortDateString() : "талон отсутствует";                       
            SelectedCommission.MKB = "МКБ: " + commissionProtocol.MKB;
            SelectedCommission.IncomeDateTime = commissionProtocol.IncomeDateTime.ToShortDateString();
            SelectedCommission.IsCompleted = commissionProtocol.IsCompleted;
            SelectedCommission.DecisionColorHex = commissionService.GetDecisionColorHex(commissionProtocol.DecisionId);
            completionTaskSource.SetResult(true);
            return true;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }
    }
}
