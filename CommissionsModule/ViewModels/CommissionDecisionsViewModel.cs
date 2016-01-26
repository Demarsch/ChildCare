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
        private readonly CommandWrapper reloadCommissionDecisionCommandWrapper;

        private readonly Decision unselectedDecision;

        private CancellationTokenSource currentOperationToken;

        private readonly Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory;
        #endregion

        #region Constructors
        public CommissionDecisionsViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IUserService userService, IEventAggregator eventAggregator,
            CommissionDecisionEditorViewModel commissionDecisionEditorViewModel, Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory)
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
            if (commissionDecisionViewModelFactory == null)
            {
                throw new ArgumentNullException("commissionDecisionViewModelFactory");
            }
            this.commissionDecisionViewModelFactory = commissionDecisionViewModelFactory;
            this.userService = userService;
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            this.CommissionDecisionEditorViewModel = commissionDecisionEditorViewModel;
            unselectedDecision = new Decision { Name = "Выберите решение" };
            reloadCommissionDecisionsCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadCommissionDecisionsAsync()), CommandName = "Повторить" };
            reloadCommissionDecisionCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadCommissionDecisionDataAsync()), CommandName = "Повторить" };
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

        private CommissionDecisionViewModel selectedCommissionDecision;
        public CommissionDecisionViewModel SelectedCommissionDecision
        {
            get { return selectedCommissionDecision; }
            set
            {
                if (SetProperty(ref selectedCommissionDecision, value))
                    LoadCommissionDecisionDataAsync();
            }
        }

        public ObservableCollectionEx<CommissionDecisionViewModel> CommissionDecisions { get; set; }

        public CommissionDecisionEditorViewModel CommissionDecisionEditorViewModel { get; set; }
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
                var commissionDecisions = await commissionDecisionsQuery/*.Where(x => x.DecisionId.HasValue)*/.OrderBy(x => x.CommissionStage).ThenBy(x => x.DecisionDateTime)
                    .Select(x => new
                    {
                        CommissionDecisionId = x.Id,
                        DecisionDate = x.DecisionDateTime,
                        Stage = x.CommissionStage,
                        MemberName = x.CommissionMember.PersonStaffId.HasValue ? x.CommissionMember.PersonStaff.Staff.ShortName + " - " + x.CommissionMember.PersonStaff.Person.ShortName :
                            x.CommissionMember.StaffId.HasValue ? x.CommissionMember.Staff.ShortName : string.Empty,
                        Decision = x.DecisionId.HasValue ? x.Decision.Name : string.Empty,
                        ColorType = x.DecisionId.HasValue && x.Decision.ColorSettingsId.HasValue ? x.Decision.ColorsSetting.Hex : "#000000"
                    })
                    .ToArrayAsync(token);
                foreach (var commissionDecision in commissionDecisions)
                {
                    var commissionDecisionViewModel = commissionDecisionViewModelFactory();
                    commissionDecisionViewModel.Initialize(commissionDecision.CommissionDecisionId, commissionDecision.DecisionDate, commissionDecision.Stage, commissionDecision.MemberName, commissionDecision.Decision, commissionDecision.ColorType);
                    CommissionDecisions.Add(commissionDecisionViewModel);
                }
                
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
            logService.InfoFormat("Loading сommission decision with id={0}...", SelectedCommissionDecision.CommissionDecisionId);
            IDisposableQueryable<CommissionDecision> commissionDecisionQuery = null;
            var curDate = DateTime.Now;
            try
            {
                commissionDecisionQuery = commissionService.GetCommissionDecision(SelectedCommissionDecision.CommissionDecisionId);
                var commissionDecision = await commissionDecisionQuery.Select(x => new
                    {
                        x.Decision,
                        Comment = x.Comment,
                        DecisionDateTime = x.DecisionDateTime,
                        CommissionQuestionId = x.CommissionProtocol.CommissionQuestionId,
                        CommissionTypeMemberId = x.CommissionMember.CommissionMemberTypeId

                    }).FirstOrDefaultAsync(token);
                var decisionsList = await Task.Factory.StartNew<IEnumerable<Decision>>(commissionService.GetDecisions, new int[] { commissionDecision.CommissionQuestionId, commissionDecision.CommissionTypeMemberId });
                var decisions = new[] { unselectedDecision }.Concat(decisionsList).ToArray();
                logService.InfoFormat("Loaded {0} decisions", (decisions as Decision[]).Length);

                CommissionDecisionEditorViewModel.Initialize(commissionDecision.Comment, commissionDecision.DecisionDateTime, commissionDecision.Decision, decisions);
                loadingIsCompleted = true;


            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load сommission decision with id={0}...", SelectedCommissionDecision.CommissionDecisionId);
                FailureMediator.Activate("Не удалость загрузить данные решения. Попробуйте еще раз или обратитесь в службу поддержки", reloadCommissionDecisionCommandWrapper, ex, true);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (commissionDecisionQuery != null)
                {
                    commissionDecisionQuery.Dispose();
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
