using CommissionsModule.Services;
using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Extensions;
using System.Data.Entity;
using Prism.Commands;

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionEditorViewModel : TrackableBindableBase
    {
        #region Fields

        private readonly ICommissionService commissionService;
        private readonly ILog logService;

        private readonly Decision unselectedDecision;

        private CommandWrapper reloadCommissionDecisionCommandWrapper;

        private CancellationTokenSource currentOperationToken;
        #endregion

        #region Constructors
        public CommissionDecisionEditorViewModel(ICommissionService commissionService, ILog logService)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            this.commissionService = commissionService;
            this.logService = logService;

            reloadCommissionDecisionCommandWrapper = new CommandWrapper() { Command = new DelegateCommand<int?>(Initialize), CommandParameter = CommissionDecisionId, CommandName = "Повторить" };
            unselectedDecision = new Decision { Name = "Выберите решение" };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();

            Decisions = new ObservableCollectionEx<Decision>();
        }
        #endregion

        #region Properties
        public ObservableCollectionEx<Decision> Decisions { get; set; }

        private int commissionDecisionId;
        public int CommissionDecisionId
        {
            get { return commissionDecisionId; }
            private set { SetProperty(ref commissionDecisionId, value); }
        }

        private Decision selectedDecision;
        public Decision SelectedDecision
        {
            get { return selectedDecision; }
            set
            {
                value = SelectDecision(value, Decisions);
                SetTrackedProperty(ref selectedDecision, value);
            }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { SetTrackedProperty(ref comment, value); }
        }

        private bool needDecisionDateTime;
        public bool NeedDecisionDateTime
        {
            get { return needDecisionDateTime; }
            set
            {
                SetTrackedProperty(ref needDecisionDateTime, value);
                if (!value)
                    DecisionDateTime = null;
            }
        }

        private DateTime? decisionDateTime;
        public DateTime? DecisionDateTime
        {
            get { return decisionDateTime; }
            set { SetTrackedProperty(ref decisionDateTime, value); }
        }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }

        #endregion

        #region Methods

        public async void Initialize(int? commissionDecisionId)
        {
            var commDecisionId = commissionDecisionId.ToInt();
            Decisions.Clear();
            if (commDecisionId < 1)
                return;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionDecisionId = commDecisionId;
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка редактора для решения протокола комиссии...");
            logService.InfoFormat("Loading сommission decision editor with decision id={0}...", commDecisionId);
            IDisposableQueryable<CommissionDecision> commissionDecisionsQuery = null;
            var curDate = DateTime.Now;
            try
            {
                commissionDecisionsQuery = commissionService.GetCommissionDecision(commDecisionId);
                var commissionDecision = await commissionDecisionsQuery.Select(x => new
                    {
                        x.Comment,
                        x.DecisionDateTime,
                        x.Decision,
                        x.CommissionProtocol.CommissionQuestionId,
                        x.CommissionMember.CommissionMemberTypeId
                    })
                    .FirstOrDefaultAsync(token);
                var decisionsList = await Task.Factory.StartNew<IEnumerable<Decision>>(commissionService.GetDecisions, new int[] { commissionDecision.CommissionQuestionId, commissionDecision.CommissionMemberTypeId });
                var decisions = new[] { unselectedDecision }.Concat(decisionsList).ToArray();
                logService.InfoFormat("Loaded {0} decisions", (decisions as Decision[]).Length);
                Decisions.AddRange(decisions);

                Comment = commissionDecision.Comment;
                DecisionDateTime = commissionDecision.DecisionDateTime;
                NeedDecisionDateTime = commissionDecision.DecisionDateTime.HasValue;
                SelectedDecision = commissionDecision.Decision;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load сommission decision with id={0}...", commDecisionId);
                FailureMediator.Activate("Не удалость загрузить редактор для решения комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reloadCommissionDecisionCommandWrapper, ex, true);
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

        private Decision SelectDecision(Decision decision, ICollection<Decision> curLevelDecisions)
        {
            Decision returnDecision = null;
            if (decision == null || curLevelDecisions == null || curLevelDecisions.Count < 1) return null;
            foreach (var curDecision in curLevelDecisions)
            {
                if (curDecision.Id == decision.Id)
                    return curDecision;
                if (curDecision.Decisions1 != null && curDecision.Decisions1.Any())
                    returnDecision = SelectDecision(decision, curDecision.Decisions1);
            }
            return returnDecision;
        }

        #endregion
    }
}
