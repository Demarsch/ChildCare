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
using System.Data.Entity;

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
            Commissions = new ObservableCollectionEx<CommissionItemViewModel>();
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

        public ObservableCollectionEx<CommissionItemViewModel> Commissions { get; set; }

        private CommissionItemViewModel selectedCommission;
        public CommissionItemViewModel SelectedCommission
        {
            get { return selectedCommission; }
            set
            {
                if (SetProperty(ref selectedCommission, value))
                {
                    if (value != null)
                        eventAggregator.GetEvent<PubSubEvent<int>>().Publish(value.Id);
                    else
                        eventAggregator.GetEvent<PubSubEvent<int>>().Publish(SpecialValues.NonExistingId);
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
                    PersonId = x.PersonId,
                    PatientFIO = x.Person.ShortName,
                    BirthDate = x.Person.BirthDate.Year,
                    Talon = x.PersonTalon != null ? new { TalonNumber = x.PersonTalon.TalonNumber, TalonDate = x.PersonTalon.TalonDateTime } : null,
                    MKB = x.MKB,
                    CommissionDate = x.CommissionDate,
                    IsCompleted = x.IsCompleted,
                    DecisionId = x.DecisionId,
                    Question = x.CommissionQuestion.ShortName
                }).ToArrayAsync();

                var result = commissionProtocolsSelectQuery.Select(x => new CommissionItemViewModel()
                {
                    Id = x.Id,
                    PersonId = x.PersonId,
                    PatientFIO = x.PatientFIO,
                    BirthDate = x.BirthDate + " г.р.",
                    Talon = x.Talon != null ? x.Talon.TalonNumber + " от " + x.Talon.TalonDate.ToShortDateString() : "талон отсутствует",
                    MKB = !string.IsNullOrEmpty(x.MKB) ? "МКБ: " + x.MKB : string.Empty,
                    CommissionDate = x.CommissionDate.ToShortDateString(),
                    IsCompleted = x.IsCompleted,
                    DecisionText = x.DecisionId.HasValue ? commissionService.GetDecisionById(x.DecisionId.Value).Name : "не рассмотрено",
                    DecisionColorHex = commissionService.GetDecisionColorHex(x.DecisionId),
                    Question = x.Question
                }).ToArray();

                Commissions.AddRange(result);

                if (!result.Any())
                    SelectedCommission = null;

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

            var filter = commissionService.GetCommissionFilterById(SelectedFilter);

            if (filter.Options.Contains(OptionValues.ProtocolsInProcess))
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => x.IsCompleted == false && !x.RemovedByUserId.HasValue);
            if (filter.Options.Contains(OptionValues.ProtocolsPreliminary))
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => x.IsCompleted == null && !x.RemovedByUserId.HasValue);
            if (filter.Options.Contains(OptionValues.ProtocolsOnCommission))
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => x.IsCompleted == false && x.IsExecuting == true && !x.RemovedByUserId.HasValue);
            if (filter.Options.Contains(OptionValues.ProtocolsOnDate) && FilterDate.HasValue)
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => DbFunctions.TruncateTime(x.CommissionDate) == DbFunctions.TruncateTime(FilterDate.Value) && !x.RemovedByUserId.HasValue);
            if (filter.Options.Contains(OptionValues.ProtocolsAdded) && FilterDate.HasValue)
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => DbFunctions.TruncateTime(x.IncomeDateTime) == DbFunctions.TruncateTime(FilterDate.Value) && !x.RemovedByUserId.HasValue);
            if (filter.Options.Contains(OptionValues.ProtocolsAwaiting))
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => x.IsCompleted == true && DbFunctions.TruncateTime(x.ToDoDateTime) > DbFunctions.TruncateTime(DateTime.Now) && !x.RemovedByUserId.HasValue);
            if (filter.Options.Contains(OptionValues.SentOnCommission))
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => x.IsSended && !x.RemovedByUserId.HasValue);

            if (OnlyMyCommissions)
            {
                int currentPersonId = userService.GetCurrentUser().PersonId;
                comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => x.CommissionDecisions.Any(a => a.CommissionMember.PersonStaff.PersonId == currentPersonId) && !x.RemovedByUserId.HasValue);
            }

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
            if (e.IsDelete)
                Commissions.Remove(Commissions.FirstOrDefault(x => x.Id == e.OldItem.Id));
            if (e.IsUpdate)
                await UpdateCommissionProtocolAsync(e.NewItem.Id);
            if (e.IsCreate)
                await CreateCommissionProtocolAsync(e.NewItem.Id);
        }

        private async Task<bool> CreateCommissionProtocolAsync(int protocolId)
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();
            var commissionProtocol = commissionService.GetCommissionProtocolById(protocolId).First();
            CommissionItemViewModel protocol = new CommissionItemViewModel();
            protocol.PersonId = commissionProtocol.PersonId;
            protocol.BirthDate = commissionProtocol.Person.BirthDate + " г.р.";
            protocol.Talon = commissionProtocol.PersonTalon != null ? commissionProtocol.PersonTalon.TalonNumber + " от " + commissionProtocol.PersonTalon.TalonDateTime.ToShortDateString() : "талон отсутствует";
            protocol.MKB = (string.IsNullOrEmpty(commissionProtocol.MKB) ? "МКБ: " + commissionProtocol.MKB : string.Empty);
            protocol.CommissionDate = commissionProtocol.IncomeDateTime.ToShortDateString();
            protocol.IsCompleted = commissionProtocol.IsCompleted;
            protocol.DecisionColorHex = commissionService.GetDecisionColorHex(commissionProtocol.DecisionId);
            protocol.DecisionText = commissionProtocol.DecisionId.HasValue ? commissionService.GetDecisionById(commissionProtocol.DecisionId.Value).Name : "не рассмотрено";
            protocol.Question = commissionProtocol.CommissionQuestion.ShortName;

            Commissions.Add(protocol);
            completionTaskSource.SetResult(true);
            return true;
        }

        private async Task<bool> UpdateCommissionProtocolAsync(int protocolId)
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();
            var commissionProtocol = commissionService.GetCommissionProtocolById(protocolId).First();
            var updateCommission = Commissions.FirstOrDefault(x => x.Id == protocolId);
            if (updateCommission != null)
            {
                updateCommission.BirthDate = commissionProtocol.Person.BirthDate + " г.р.";
                updateCommission.Talon = commissionProtocol.PersonTalon != null ? commissionProtocol.PersonTalon.TalonNumber + " от " + commissionProtocol.PersonTalon.TalonDateTime.ToShortDateString() : "талон отсутствует";
                updateCommission.MKB = (string.IsNullOrEmpty(commissionProtocol.MKB) ? "МКБ: " + commissionProtocol.MKB : string.Empty);
                updateCommission.CommissionDate = commissionProtocol.IncomeDateTime.ToShortDateString();
                updateCommission.IsCompleted = commissionProtocol.IsCompleted;
                updateCommission.DecisionColorHex = commissionService.GetDecisionColorHex(commissionProtocol.DecisionId);
                updateCommission.DecisionText = commissionProtocol.DecisionId.HasValue ? commissionService.GetDecisionById(commissionProtocol.DecisionId.Value).Name : "не рассмотрено";
                updateCommission.Question = commissionProtocol.CommissionQuestion.ShortName;
            }
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
