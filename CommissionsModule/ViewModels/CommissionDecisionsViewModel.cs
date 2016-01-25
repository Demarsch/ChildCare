using CommissionsModule.Services;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using Core.Extensions;
using Core.Wpf.Misc;
using Prism.Commands;

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionsViewModel : BindableBase, INavigationAware
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly IDialogServiceAsync dialogService;
        private readonly ILog logService;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;

        private readonly CommandWrapper reloadCommissionDecisionsCommandWrapper;

        private CancellationTokenSource currentOperationToken;
        #endregion

        #region Constructors
        public CommissionDecisionsViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IUserService userService, IEventAggregator eventAggregator)
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
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.userService = userService;
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            reloadCommissionDecisionsCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadCommissionDecisionsAsync()), CommandName = "Повторить" };
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            CommissionDecisions = new ObservableCollectionEx<CommissionDecisionViewModel>();
            SelectedCommissionId = 3;
        }
        #endregion

        #region Properties
        private int selectedCommissionId = 0;
        public int SelectedCommissionId
        {
            get { return selectedCommissionId; }
            set
            {
                if (SetProperty(ref selectedCommissionId, value))
                    LoadCommissionDecisionsAsync();
            }
        }

        private ObservableCollectionEx<CommonIdName> decisions;
        public ObservableCollectionEx<CommonIdName> Decisions
        {
            get { return decisions; }
            set { SetProperty(ref decisions, value); }
        }

        private int selectedCommissionDecisionId;
        public int SelectedCommissionDecisionId
        {
            get { return selectedCommissionDecisionId; }
            set
            {
                SetProperty(ref selectedCommissionDecisionId, value);
                LoadCommissionDecisionDataAsync();
            }
        }

        public ObservableCollectionEx<CommissionDecisionViewModel> CommissionDecisions { get; set; }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        #endregion

        #region Methods

        private async void LoadCommissionDecisionsAsync()
        {
            CommissionDecisions.Clear();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка решений протокола комиссии...");
            logService.InfoFormat("Loading сommission decisions with commission id={0}...", SelectedCommissionId);
            IDisposableQueryable<CommissionDecision> commissionDecisionsQuery = null;
            var curDate = DateTime.Now;
            try
            {
                commissionDecisionsQuery = commissionService.GetCommissionDecisions(SelectedCommissionId);
                var commissionDecisions = await commissionDecisionsQuery/*.Where(x => x.DecisionId.HasValue)*/.OrderBy(x => x.DecisionDateTime)
                    .Select(x => new CommissionDecisionViewModel()
                    {
                        DecisionDate = x.DecisionDateTime,
                        MemberName = x.CommissionMember.PersonStaffId.HasValue ? x.CommissionMember.PersonStaff.Staff.ShortName + " - " + x.CommissionMember.PersonStaff.Person.ShortName :
                            x.CommissionMember.StaffId.HasValue ? x.CommissionMember.Staff.ShortName : string.Empty,
                        Decision = x.DecisionId.HasValue ? x.Decision.Name : string.Empty,
                        ColorType = x.DecisionId.HasValue && x.Decision.ColorSettingsId.HasValue ? x.Decision.ColorsSetting.Hex : "#000000"
                    })
                    .ToArrayAsync(token);
                CommissionDecisions.AddRange(commissionDecisions);
                //Load recordPeriods
                //recordPeriodsQuery = patientRecordsService.GetActualRecordPeriods(data.ExecutionPlaceId, data.BeginDateTime);
                //var recordPeriods = await recordPeriodsQuery.Select(x => new CommonIdName { Id = x.Id, Name = x.Name }).ToArrayAsync(token);
                //RecordPeriods.AddRange(recordPeriods);
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load сommission decisions with commission id={0}...", SelectedCommissionId);
                FailureMediator.Activate("Не удалость загрузить решения комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reloadCommissionDecisionsCommandWrapper, ex, true);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (commissionDecisionsQuery != null)
                {
                    commissionDecisionsQuery.Dispose();
                }
            }
        }

        private async void LoadCommissionDecisionDataAsync()
        {
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка редактора решения...");
            logService.InfoFormat("Loading сommission decision with id={0}...", SelectedCommissionDecisionId);
            IDisposableQueryable<CommissionDecision> commissionDecisionQuery = null;
            var curDate = DateTime.Now;
            try
            {
                //commissionDecisionQuery = commissionService.GetCommissionDecision(SelectedCommissionDecisionId);
                //var commissionDecisions = await commissionDecisionsQuery/*.Where(x => x.DecisionId.HasValue)*/.OrderBy(x => x.InDateTime)
                //    .Select(x => new CommissionDecisionViewModel()
                //    {
                //        DecisionDate = x.InDateTime,
                //        MemberName = x.CommissionMember.PersonStaffId.HasValue ? x.CommissionMember.PersonStaff.Staff.ShortName + " - " + x.CommissionMember.PersonStaff.Person.ShortName :
                //            x.CommissionMember.StaffId.HasValue ? x.CommissionMember.Staff.ShortName : string.Empty,
                //        Decision = x.DecisionId.HasValue ? x.Decision.Name : string.Empty,
                //        ColorType = x.DecisionId.HasValue && x.Decision.ColorSettingsId.HasValue ? x.Decision.ColorsSetting.Hex : "#000000"
                //    })
                //    .ToArrayAsync(token);
                //CommissionDecisions.AddRange(commissionDecisions);
                //Load recordPeriods
                //recordPeriodsQuery = patientRecordsService.GetActualRecordPeriods(data.ExecutionPlaceId, data.BeginDateTime);
                //var recordPeriods = await recordPeriodsQuery.Select(x => new CommonIdName { Id = x.Id, Name = x.Name }).ToArrayAsync(token);
                //RecordPeriods.AddRange(recordPeriods);
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load сommission decisions with id={0}...", SelectedCommissionDecisionId);
                FailureMediator.Activate("Не удалость загрузить решения комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reloadCommissionDecisionsCommandWrapper, ex, true);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                //if (commissionDecisionsQuery != null)
                //{
                //    commissionDecisionsQuery.Dispose();
                //}
            }
        }
        #endregion

        #region INavigationAware implementations
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            return;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            return;
        }
        #endregion
    }
}
