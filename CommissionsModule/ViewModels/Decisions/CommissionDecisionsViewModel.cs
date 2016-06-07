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
using System.Windows.Input;
using Core.Services;

namespace CommissionsModule.ViewModels
{
    public class CommissionDecisionsViewModel : BindableBase, INavigationAware, IDisposable, IChangeTrackerMediator
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly IDialogServiceAsync dialogService;
        private readonly ILog logService;
        private readonly IUserService userService;
        private readonly ISecurityService securityService;
        private readonly IEventAggregator eventAggregator;

        private readonly CommandWrapper reloadCommissionDecisionsCommandWrapper;
        private readonly CommandWrapper saveDecisionCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        private readonly Func<CommissionDecisionViewModel> commissionDecisionViewModelFactory;
        #endregion

        #region Constructors
        public CommissionDecisionsViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IUserService userService, ISecurityService securityService, IEventAggregator eventAggregator,
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
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            this.securityService = securityService;
            this.commissionDecisionViewModelFactory = commissionDecisionViewModelFactory;
            this.userService = userService;
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            this.CommissionDecisionEditorViewModel = commissionDecisionEditorViewModel;

            SubscribeToEvents();

            reloadCommissionDecisionsCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => LoadCommissionDecisionsAsync()), CommandName = "Повторить" };
            saveDecisionCommandWrapper = new CommandWrapper() { Command = saveDecisionCommand, CommandName = "Повторить" };
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            NotificationMediator = new NotificationMediator();
            CompositeChangeTracker = new ChangeTrackerEx<CommissionDecisionEditorViewModel>(CommissionDecisionEditorViewModel);
            CompositeChangeTracker.PropertyChanged += ChangeTracker_PropertyChanged;
            CommissionDecisions = new ObservableCollectionEx<CommissionDecisionViewModel>();
            saveDecisionCommand = new DelegateCommand(SaveDecision, CanSaveDecision);
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
                if (SetProperty(ref selectedCommissionDecision, value) && SelectedCommissionDecision != null)
                {
                    CommissionDecisionEditorViewModel.CompositeChangeTracker = CompositeChangeTracker;
                    CommissionDecisionEditorViewModel.Initialize(SelectedCommissionDecision.CommissionDecisionId);
                }
            }
        }

        public ObservableCollectionEx<CommissionDecisionViewModel> CommissionDecisions { get; set; }

        public CommissionDecisionEditorViewModel CommissionDecisionEditorViewModel { get; set; }
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        public NotificationMediator NotificationMediator { get; set; }
        public IChangeTracker CompositeChangeTracker { get; set; }
        #endregion

        #region Methods

        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<PubSubEvent<int>>().Unsubscribe(OnCommissionProtocolSelected);
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<PubSubEvent<int>>().Subscribe(OnCommissionProtocolSelected);
        }

        private void OnCommissionProtocolSelected(int protocolId)
        {
            SelectedCommissionId = 0;
            if (!SpecialValues.IsNewOrNonExisting(protocolId))
                SelectedCommissionId = protocolId;
        }

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
                var showAllCommissionDecisions = securityService.HasPermission(Permission.ShowAllCommissionDecisions);
                var currentUserPersonStaffIdsTask = Task.Run((Func<IEnumerable<int>>)userService.GetCurrentUserPersonStaffIds);
                var currentUserStaffIdsTask = Task.Run((Func<IEnumerable<int>>)userService.GetCurrentUserStaffIds);
                await Task.WhenAll(currentUserPersonStaffIdsTask, currentUserStaffIdsTask);
                var currentUserPersonStaffIds = currentUserPersonStaffIdsTask.Result;
                var currentUserStaffIds = currentUserStaffIdsTask.Result;
                var commissionDecisionIds = await commissionDecisionsQuery
                    .Where(x => showAllCommissionDecisions || (x.DecisionId.HasValue || (currentUserPersonStaffIds.Contains(x.CommissionMember.PersonStaffId ?? 0) || currentUserStaffIds.Contains(x.CommissionMember.StaffId ?? 0))))
                    .OrderBy(x => x.CommissionStage).ThenBy(x => x.DecisionDateTime ?? SpecialValues.MaxDate)
                    .Select(x => x.Id).ToArrayAsync(token);
                foreach (var commissionDecisionId in commissionDecisionIds)
                {
                    var commissionDecisionViewModel = commissionDecisionViewModelFactory();
                    await commissionDecisionViewModel.Initialize(commissionDecisionId);
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

        private bool CanSaveDecision()
        {
            if (CommissionDecisionEditorViewModel == null || SelectedCommissionDecision == null)
            {
                return false;
            }
            return CompositeChangeTracker.HasChanges;
        }

        private async void SaveDecision()
        {
            logService.Info("Saving commission decision...");
            FailureMediator.Deactivate();
            BusyMediator.Activate("Сохранение данных...");
            currentOperationToken = new CancellationTokenSource();
            var savedSuccessfull = false;
            var token = currentOperationToken.Token;
            try
            {
                //CommissionDecisionEditorViewModel
                var error = await commissionService.SaveDecision(CommissionDecisionEditorViewModel.CommissionDecisionId, CommissionDecisionEditorViewModel.SelectedDecision.Id, CommissionDecisionEditorViewModel.Comment,
                     CommissionDecisionEditorViewModel.ToDoDecisionDateTime, token);
                if (error != string.Empty)
                {
                    logService.ErrorFormat("Failed to save commission decision with error {0}", error);
                    FailureMediator.Activate("При попытке сохранить решение для комиссии возникла ошибка:" + error + ". Попробуйте еще раз, если ошибка повторится, обратитесь в службу поддержки",
                        saveDecisionCommandWrapper, canBeDeactivated: true);
                }
                else
                {
                    savedSuccessfull = true;
                    await SelectedCommissionDecision.Initialize(CommissionDecisionEditorViewModel.CommissionDecisionId);
                }
            }
            catch (Exception ex)
            {
                logService.Error("Failed to save commission decision", ex);
                FailureMediator.Activate("ППри попытке сохранить решение для комиссии возникла ошибка. Попробуйте еще раз, если ошибка повторится, обратитесь в службу поддержки", saveDecisionCommandWrapper, ex, true);
            }
            finally
            {
                if (savedSuccessfull)
                {
                    CompositeChangeTracker.AcceptChanges();
                    NotificationMediator.Activate("Данные сохранены.", NotificationMediator.DefaultHideTime);
                }
                BusyMediator.Deactivate();
            }
        }
        #endregion

        #region Events

        void ChangeTracker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                saveDecisionCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands
        private DelegateCommand saveDecisionCommand;
        public ICommand SaveDecisionCommand { get { return saveDecisionCommand; } }
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
