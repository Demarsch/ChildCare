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
using Core.Data.Services;
using Core.Misc;
using System.ComponentModel;

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionEditorViewModel : TrackableBindableBase, IChangeTrackerMediator, IDataErrorInfo
    {
        #region Fields

        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IUserService userService;

        private readonly Decision unselectedDecision;

        private CommandWrapper reloadCommissionDecisionCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        private readonly ValidationMediator validator;
        #endregion

        #region Constructors
        public CommissionDecisionEditorViewModel(ICommissionService commissionService, ILog logService, IUserService userService)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            this.userService = userService;
            this.commissionService = commissionService;
            this.logService = logService;

            reloadCommissionDecisionCommandWrapper = new CommandWrapper() { Command = new DelegateCommand<int?>(Initialize), CommandParameter = CommissionDecisionId, CommandName = "Повторить" };
            unselectedDecision = new Decision { Name = "Выберите решение" };

            validator = new ValidationMediator(this);

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
            private set { SetTrackedProperty(ref commissionDecisionId, value); }
        }

        private Decision selectedDecision;
        public Decision SelectedDecision
        {
            get { return selectedDecision; }
            set
            {

                if (value == null)
                    value = unselectedDecision;
                else
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

        private bool needtoDoDecisionDateTime;
        public bool NeedToDoDecisionDateTime
        {
            get { return needtoDoDecisionDateTime; }
            set
            {
                SetTrackedProperty(ref needtoDoDecisionDateTime, value);
                if (!value)
                    ToDoDecisionDateTime = null;
            }
        }

        private DateTime? toDoDecisionDateTime;
        public DateTime? ToDoDecisionDateTime
        {
            get { return toDoDecisionDateTime; }
            set { SetTrackedProperty(ref toDoDecisionDateTime, value); }
        }

        private bool canEdit;
        public bool CanEdit
        {
            get { return canEdit; }
            set { SetProperty(ref canEdit, value); }
        }

        private string errorText;
        public string ErrorText
        {
            get { return errorText; }
            set { SetProperty(ref errorText, value); }
        }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        public IChangeTracker ChangeTracker { get; set; }

        #endregion

        #region Methods

        public async void Initialize(int? commissionDecisionId)
        {
            var commDecisionId = commissionDecisionId.ToInt();
            ErrorText = string.Empty;
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
                        x.CommissionMember.CommissionMemberTypeId,
                        x.CommissionStage,
                        PersonStaffId = x.CommissionMember.PersonStaffId ?? 0,
                        StaffId = x.CommissionMember.StaffId ?? 0,
                        CommissionProtocolIsCompleted = x.CommissionProtocol.IsCompleted,
                        CommissionDecisions = x.CommissionProtocol.CommissionDecisions.Select(y => new { y.CommissionStage, y.NeedAllMembersInStage, HasDecision = y.DecisionId.HasValue })
                    })
                    .FirstOrDefaultAsync(token);
                var decisionsList = await Task.Factory.StartNew<IEnumerable<Decision>>(commissionService.GetDecisions,
                    new Tuple<int, int, DateTime>(commissionDecision.CommissionQuestionId, commissionDecision.CommissionMemberTypeId, commissionDecision.DecisionDateTime ?? DateTime.Now.Date));
                var decisions = new[] { unselectedDecision }.Concat(decisionsList).ToArray();
                logService.InfoFormat("Loaded {0} decisions", (decisions as Decision[]).Length);
                Decisions.AddRange(decisions);
                // editing ability
                var currentUserPersonStaffIdsTask = Task.Run((Func<IEnumerable<int>>)userService.GetCurrentUserPersonStaffIds);
                var currentUserStaffIdsTask = Task.Run((Func<IEnumerable<int>>)userService.GetCurrentUserStaffIds);
                await Task.WhenAll(currentUserPersonStaffIdsTask, currentUserStaffIdsTask);
                var currentUserPersonStaffIds = currentUserPersonStaffIdsTask.Result;
                var currentUserStaffIds = currentUserStaffIdsTask.Result;
                int curStage = -1;
                var rStage = commissionDecision.CommissionDecisions.GroupBy(x => x.CommissionStage).OrderBy(x => x.Key)
                     .FirstOrDefault(x => x.Any(y => y.NeedAllMembersInStage) ? !x.All(y => y.HasDecision) : !x.Any(y => y.HasDecision));
                if (rStage != null)
                    curStage = rStage.Key;
                else
                    curStage = commissionDecision.CommissionDecisions.Max(x => x.CommissionStage);
                CanEdit = (currentUserPersonStaffIds.Contains(commissionDecision.PersonStaffId) || currentUserStaffIds.Contains(commissionDecision.StaffId)) && curStage == commissionDecision.CommissionStage &&
                    commissionDecision.CommissionProtocolIsCompleted != true;

                if (commissionDecision.CommissionProtocolIsCompleted == true)
                    ErrorText = "Комиссия уже завершена! Вынесение решения невозможно";
                else if (curStage > commissionDecision.CommissionStage)
                    ErrorText = "Данный этап комиссии уже пройден";
                else if (curStage < commissionDecision.CommissionStage)
                    ErrorText = "Данный этап комиссии еще не активен";
                else if (!currentUserPersonStaffIds.Contains(commissionDecision.PersonStaffId) && !currentUserStaffIds.Contains(commissionDecision.StaffId))
                    ErrorText = "Вы не являетесь членом комиссии, который принимает данное решение";

                // decision data
                Comment = commissionDecision.Comment;
                ToDoDecisionDateTime = commissionDecision.DecisionDateTime;
                NeedToDoDecisionDateTime = commissionDecision.DecisionDateTime.HasValue;
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
                    if (ChangeTracker != null)
                        ChangeTracker.IsEnabled = true;
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

        #region IDataErrorInfo implementation

        public string Error
        {
            get { return validator.Error; }
        }

        public string this[string columnName]
        {
            get { return validator[columnName]; }
        }

        private class ValidationMediator : ValidationMediator<CommissionDecisionEditorViewModel>
        {
            public ValidationMediator(CommissionDecisionEditorViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.SelectedDecision))
                {
                    ValidateDecision();
                }
                if (PropertyNameEquals(propertyName, x => x.ToDoDecisionDateTime))
                {
                    ValidateToDoDecisionDateTime();
                }
            }

            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateDecision();
                ValidateToDoDecisionDateTime();
            }

            private void ValidateDecision()
            {
                SetError(x => x.SelectedDecision, AssociatedItem.selectedDecision == null || AssociatedItem.SelectedDecision.Id < 1 ? "Необходимо указать решение" : string.Empty);
            }

            private void ValidateToDoDecisionDateTime()
            {
                SetError(x => x.ToDoDecisionDateTime, AssociatedItem.ToDoDecisionDateTime == null && AssociatedItem.NeedToDoDecisionDateTime ? "Необходимо указать дату, когда требуется выполнить решение" : string.Empty);
            }
        }
        #endregion
    }
}
