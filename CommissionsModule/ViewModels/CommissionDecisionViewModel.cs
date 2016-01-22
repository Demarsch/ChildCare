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
    public class CommissionDecisionViewModel : BindableBase, INavigationAware
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
        public CommissionDecisionViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IUserService userService, IEventAggregator eventAggregator)
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


        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        #endregion

        #region Methods

        private async void LoadCommissionDecisionsAsync()
        {
            Decisions.Clear();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка решений протокола комиссии...");
            logService.InfoFormat("Loading сommission decisions with id={0}...", SelectedCommissionId);
            IDisposableQueryable<CommissionDecision> commissionDecisionsQuery = null;
            var curDate = DateTime.Now;
            try
            {
                commissionDecisionsQuery = commissionService.GetCommissionDecisions(SelectedCommissionId);
                var commissionDecisions = await commissionDecisionsQuery.Where(x => x.DecisionId.HasValue).OrderBy(x => x.InDateTime).ToArrayAsync();
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
                logService.ErrorFormatEx(ex, "Failed to load сommission decisions with id={0}...", SelectedCommissionId);
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
