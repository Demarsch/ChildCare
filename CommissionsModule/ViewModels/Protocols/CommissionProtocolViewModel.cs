using CommissionsModule.Services;
using Core.Data.Misc;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Shared.Patient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Extensions;
using System.Data.Entity;
using Core.Wpf.Mvvm;
using Core.Misc;
using Core.Data;
using Core.Wpf.Misc;
using Core.Services;
using Core.Data.Services;
using Core.Notification;

namespace CommissionsModule.ViewModels
{
    public class CommissionProtocolViewModel : TrackableBindableBase, IChangeTrackerMediator, IDisposable
    {
        #region Fields

        private readonly ICommissionService commissionService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogServiceAsync dialogService;
        private readonly ILog logService;
        private readonly ISecurityService securityService;
        private readonly IUserService userService;
        private readonly INotificationService notificationService;

        private readonly Func<PersonSearchDialogViewModel> relativeSearchFactory;
        private readonly Func<EditorCommissionMembersViewModel> editorCommissionMembersFactory;

        private INotificationServiceSubscription<CommissionProtocol> comissionsProtocolsChangeSubscription;

        private CancellationTokenSource currentOperationToken;
        private TaskCompletionSource<bool> completionTaskSource;

        private CommandWrapper reSaveCommissionProtocol;
        #endregion

        #region Constructiors
        public CommissionProtocolViewModel(ICommissionService commissionService, IEventAggregator eventAggregator, IDialogServiceAsync dialogService, ILog logService,
            PreliminaryProtocolViewModel preliminaryProtocolViewModel, CommissionСonductViewModel commissionСonductViewModel, CommissionСonclusionViewModel commissionСonclusionViewModel,
            ISecurityService securityService, IUserService userService, INotificationService notificationService,
             Func<PersonSearchDialogViewModel> relativeSearchFactory, Func<EditorCommissionMembersViewModel> editorCommissionMembersFactory)
        {
            if (preliminaryProtocolViewModel == null)
            {
                throw new ArgumentNullException("preliminaryProtocolViewModel");
            }
            if (commissionСonductViewModel == null)
            {
                throw new ArgumentNullException("commissionСonductViewModel");
            }
            if (commissionСonclusionViewModel == null)
            {
                throw new ArgumentNullException("commissionСonclusionViewModel");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregatorСonclusionViewModel");
            }
            if (relativeSearchFactory == null)
            {
                throw new ArgumentNullException("relativeSearchFactory");
            }
            if (editorCommissionMembersFactory == null)
            {
                throw new ArgumentNullException("editorCommissionMembersFactory");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (notificationService == null)
            {
                throw new ArgumentNullException("notificationService");
            }
            this.notificationService = notificationService;
            this.userService = userService;
            this.commissionService = commissionService;
            this.logService = logService;
            this.dialogService = dialogService;
            this.securityService = securityService;
            this.relativeSearchFactory = relativeSearchFactory;
            this.editorCommissionMembersFactory = editorCommissionMembersFactory;
            this.eventAggregator = eventAggregator;
            PreliminaryProtocolViewModel = preliminaryProtocolViewModel;
            CommissionСonductViewModel = commissionСonductViewModel;
            CommissionСonclusionViewModel = commissionСonclusionViewModel;
            createCommissionCommand = new DelegateCommand(CreateCommission);
            saveCommissionProtocolCommand = new DelegateCommand(SaveCommissionProtocol, CanSaveCommissionProtocol);
            editCommissionMembersCommand = new DelegateCommand(EditCommissionMembers, CanEditCommissionMembers);

            addCommissionConductionCommand = new DelegateCommand<bool?>(AddCommissionConduction);
            addCommissionConclusionCommand = new DelegateCommand<bool?>(AddCommissionConclusion);

            reSaveCommissionProtocol = new CommandWrapper() { Command = saveCommissionProtocolCommand, CommandName = "Повторить" };
            SubscribeToEvents();

            FailureMediator = new FailureMediator();
            NotificationMediator = new NotificationMediator();
            ChangeTracker = new CompositeChangeTracker(PreliminaryProtocolViewModel.ChangeTracker, CommissionСonductViewModel.ChangeTracker, CommissionСonclusionViewModel.ChangeTracker);
            ChangeTracker.PropertyChanged += CompositeChangeTracker_PropertyChanged;
        }

        #endregion

        #region Properties

        public PreliminaryProtocolViewModel PreliminaryProtocolViewModel { get; set; }
        public CommissionСonductViewModel CommissionСonductViewModel { get; set; }
        public CommissionСonclusionViewModel CommissionСonclusionViewModel { get; set; }

        private int selectedCommissionProtocolId = 0;
        public int SelectedCommissionProtocolId
        {
            get { return selectedCommissionProtocolId; }
            set
            {
                SetProperty(ref selectedCommissionProtocolId, value);
                if (value != SpecialValues.NewId)
                {
                    SetCommissionProtocolState();
                    SetPatientName();
                    ShowCommissionProtocol = selectedCommissionProtocolId > 0;
                }
                SubscribeToCommissionsProtocolsChangesAsync();
            }
        }

        private int selectedPersonId = 0;
        public int SelectedPersonId
        {
            get { return selectedPersonId; }
            set
            {
                SetProperty(ref selectedPersonId, value);
                SetPatientName();
                ShowCommissionProtocol = selectedPersonId > 0;
            }
        }

        private bool commissionСonductVisible = false;
        public bool CommissionСonductVisible
        {
            get { return commissionСonductVisible; }
            set { SetProperty(ref commissionСonductVisible, value); }
        }

        private bool commissionСonclusionVisible = false;
        public bool CommissionСonclusionVisible
        {
            get { return commissionСonclusionVisible; }
            set { SetProperty(ref commissionСonclusionVisible, value); }
        }

        private CommissionProtocolState commissionProtocolState;
        public CommissionProtocolState CommissionProtocolState
        {
            get { return commissionProtocolState; }
            set
            {
                SetProperty(ref commissionProtocolState, value);
                CommissionСonclusionVisible = false;
                CommissionСonductVisible = false;
                if ((int)CommissionProtocolState > 0)
                    CommissionСonductVisible = true;
                if ((int)CommissionProtocolState > 1)
                    CommissionСonclusionVisible = true;
            }
        }

        private string patient;
        public string Patient
        {
            get { return patient; }
            set { SetProperty(ref patient, value); }
        }

        private bool showCommissionProtocol;
        public bool ShowCommissionProtocol
        {
            get { return showCommissionProtocol; }
            set { SetProperty(ref showCommissionProtocol, value); }
        }

        public FailureMediator FailureMediator { get; private set; }
        public NotificationMediator NotificationMediator { get; private set; }
        public IChangeTracker ChangeTracker { get; private set; }

        #endregion

        #region Commands
        private DelegateCommand createCommissionCommand;
        public ICommand CreateCommissionCommand { get { return createCommissionCommand; } }

        private DelegateCommand saveCommissionProtocolCommand;
        public ICommand SaveCommissionProtocolCommand { get { return saveCommissionProtocolCommand; } }

        private DelegateCommand<bool?> addCommissionConductionCommand;
        public ICommand AddCommissionConductionCommand { get { return addCommissionConductionCommand; } }

        private DelegateCommand<bool?> addCommissionConclusionCommand;
        public ICommand AddCommissionConclusionCommand { get { return addCommissionConclusionCommand; } }

        private DelegateCommand editCommissionMembersCommand;
        public ICommand EditCommissionMembersCommand { get { return editCommissionMembersCommand; } }

        #endregion

        #region Methods



        private bool CanEditCommissionMembers()
        {
            return securityService.HasPermission(Permission.EditCommissionMembers);
        }

        private void AddCommissionConduction(bool? select)
        {
            if (select == null) return;
            if (select.Value)
                CommissionProtocolState = ViewModels.CommissionProtocolState.Сonduction;
            else
                CommissionProtocolState = ViewModels.CommissionProtocolState.Preliminary;
            CommissionСonductViewModel.Initialize(SelectedCommissionProtocolId, SelectedPersonId);
        }

        private void AddCommissionConclusion(bool? select)
        {
            if (select == null) return;
            if (select.Value)
                CommissionProtocolState = ViewModels.CommissionProtocolState.Сonclusion;
            else
                CommissionProtocolState = ViewModels.CommissionProtocolState.Сonduction;
            CommissionСonclusionViewModel.Initialize(SelectedCommissionProtocolId, SelectedPersonId);
        }

        private async void SetCommissionProtocolState()
        {
            if (SelectedCommissionProtocolId < 1)
                return;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Loading commission protocol state with protocol id ={0}", SelectedCommissionProtocolId);
            var commissionProtocolQuery = commissionService.GetCommissionProtocolById(SelectedCommissionProtocolId);
            try
            {
                var commissionProtocol = await commissionProtocolQuery.Select(x => new
                {
                    x.IsCompleted,
                    x.Id
                }).FirstOrDefaultAsync(token);
                if (commissionProtocol != null)
                {
                    switch (commissionProtocol.IsCompleted)
                    {
                        case false:
                            CommissionProtocolState = CommissionProtocolState.Сonduction;
                            PreliminaryProtocolViewModel.Initialize(SelectedCommissionProtocolId);
                            CommissionСonductViewModel.Initialize(SelectedCommissionProtocolId);
                            break;
                        case true:
                            CommissionProtocolState = CommissionProtocolState.Сonclusion;
                            PreliminaryProtocolViewModel.Initialize(SelectedCommissionProtocolId);
                            CommissionСonductViewModel.Initialize(SelectedCommissionProtocolId);
                            CommissionСonclusionViewModel.Initialize(SelectedCommissionProtocolId);
                            break;
                        default:
                            CommissionProtocolState = CommissionProtocolState.Preliminary;
                            PreliminaryProtocolViewModel.Initialize(SelectedCommissionProtocolId);
                            break;
                    }
                }
                else
                {
                    FailureMediator.Activate("Не удалось загрузить состояние протокола");
                }
            }
            catch (OperationCanceledException) {/*Do nothing. Cancelled operation means that user selected different patient before previous one was loaded  */}
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission protocol state with protocol id ={0}", SelectedCommissionProtocolId);
                FailureMediator.Activate("Не удалось загрузить состояние протокола", exception: ex);
            }
            finally
            {
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
            }
        }

        private async void SetPatientName()
        {
            Patient = string.Empty;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Loading patient name with  person id ={0}", SelectedPersonId);
            var personQuery = commissionService.GetPerson(SelectedPersonId);
            var commissionprotocolQuery = commissionService.GetCommissionProtocolById(SelectedCommissionProtocolId);
            try
            {
                if (SelectedCommissionProtocolId > 0)
                {
                    var person = await commissionprotocolQuery.Select(x => new
                    {
                        x.Person.ShortName,
                        x.Person.BirthDate
                    }).FirstOrDefaultAsync(token);
                    if (person != null)
                    {
                        Patient = person.ShortName + " " + person.BirthDate.ToShortDateString() + "г.р.";
                    }
                }
                else
                {
                    var person = await personQuery.Select(x => new
                    {
                        x.ShortName,
                        x.BirthDate
                    }).FirstOrDefaultAsync(token);
                    if (person != null)
                    {
                        Patient = person.ShortName + " " + person.BirthDate.ToShortDateString() + "г.р.";
                    }
                }

                if (Patient == string.Empty)
                {
                    FailureMediator.Activate("Не удалось загрузить данные пациента");
                }
            }
            catch (OperationCanceledException) {/*Do nothing. Cancelled operation means that user selected different patient before previous one was loaded  */}
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load patient name with person id ={0}", SelectedPersonId);
                FailureMediator.Activate("Не удалось загрузить данные пациента", exception: ex);
            }
            finally
            {
                if (personQuery != null)
                    personQuery.Dispose();
                if (commissionprotocolQuery != null)
                    commissionprotocolQuery.Dispose();
            }
        }

        private async void CreateCommission()
        {
            using (var searchViewModel = relativeSearchFactory())
            {
                var result = await dialogService.ShowDialogAsync(searchViewModel);
                if (result != true)
                {
                    return;
                }
                SelectedCommissionProtocolId = SpecialValues.NewId;
                CommissionProtocolState = CommissionProtocolState.Preliminary;
                SelectedPersonId = searchViewModel.PersonSearchViewModel.SelectedPersonId;
                PreliminaryProtocolViewModel.Initialize(SelectedCommissionProtocolId, SelectedPersonId);
            }
        }

        private async void SaveCommissionProtocol()
        {
            FailureMediator.Deactivate();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Saving commission protocol with id = {0} for patient with id = {1}", SelectedCommissionProtocolId, SelectedPersonId);
            IDisposableQueryable<CommissionDecision> commissionDecisionQuery = commissionService.GetCommissionDecisions(SelectedCommissionProtocolId);
            try
            {
                var commissionProtocol = new CommissionProtocol()
                 {
                     Id = SelectedCommissionProtocolId,
                     PersonId = SelectedPersonId,
                     ProtocolNumber = 0,
                     ProtocolDate = DateTime.Now,
                     IsCompleted = CommissionProtocolState == ViewModels.CommissionProtocolState.Сonduction ? false :
                        CommissionProtocolState == ViewModels.CommissionProtocolState.Сonduction ? true : (bool?)null,
                     InUserId = userService.GetCurrentUserId(),
                     Comment = string.Empty,
                     MKB = string.Empty,
                     Diagnos = string.Empty,
                     WaitingFor = string.Empty,
                     IsExecuting = false
                     //CommissionDecisions = await commissionDecisionQuery.ToArrayAsync(token)
                 };
                //ToDo: Maybe change on better decision!
                switch (CommissionProtocolState)
                {
                    case CommissionProtocolState.Сonclusion:
                        PreliminaryProtocolViewModel.GetPreliminaryCommissionProtocolData(ref commissionProtocol);
                        CommissionСonductViewModel.GetСonductionCommissionProtocolData(ref commissionProtocol);
                        CommissionСonclusionViewModel.GetСonclusionCommissionProtocolData(ref commissionProtocol);
                        break;
                    case CommissionProtocolState.Сonduction:
                        PreliminaryProtocolViewModel.GetPreliminaryCommissionProtocolData(ref commissionProtocol);
                        CommissionСonductViewModel.GetСonductionCommissionProtocolData(ref commissionProtocol);
                        break;
                    case CommissionProtocolState.Preliminary:
                        PreliminaryProtocolViewModel.GetPreliminaryCommissionProtocolData(ref commissionProtocol);
                        break;
                    default:
                        break;
                }
                await commissionService.SaveCommissionProtocolAsync(commissionProtocol, token, comissionsProtocolsChangeSubscription);
                ChangeTracker.AcceptChanges();
            }
            catch (OperationCanceledException) {/*Do nothing. Cancelled operation means that user selected different patient before previous one was loaded  */}
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save commission protocol with id = {0} for patient with id = {1}", SelectedCommissionProtocolId, SelectedPersonId);
                FailureMediator.Activate("Не удалось сохранить протокол комиссии", reSaveCommissionProtocol, ex, true);
            }
            finally
            {
                NotificationMediator.Activate("Данные сохранены...");
            }
        }
        private async void SubscribeToCommissionsProtocolsChangesAsync()
        {
            await SubscribeToCommissionsProtocolsChanges();
        }

        private async Task<bool> SubscribeToCommissionsProtocolsChanges()
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();
            comissionsProtocolsChangeSubscription = notificationService.Subscribe<CommissionProtocol>(x => x.Id == SelectedCommissionProtocolId);
            if (comissionsProtocolsChangeSubscription != null)
                comissionsProtocolsChangeSubscription.Notified += OnCommissionProtocolNotificationRecievedAsync;
            completionTaskSource.SetResult(true);
            return true;
        }

        private void OnCommissionProtocolNotificationRecievedAsync(object sender, NotificationEventArgs<CommissionProtocol> e)
        {

        }

        private async void EditCommissionMembers()
        {
            var editCommisionMembersViewModel = editorCommissionMembersFactory();
            editCommisionMembersViewModel.Initialize();
            var result = await dialogService.ShowDialogAsync(editCommisionMembersViewModel);
            /*if (result == true && (editCommisionMembersViewModel.IsChanged || editCommisionMembersViewModel.Members.Any(x => x.IsChanged)))
            {
                
            }*/
        }

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
            //SelectedCommissionProtocolId = protocolId;
            if (!SpecialValues.IsNewOrNonExisting(protocolId))
                SelectedCommissionProtocolId = protocolId;
        }

        private bool CanSaveCommissionProtocol()
        {
            return ChangeTracker.HasChanges;
        }

        #endregion

        #region Events

        void CompositeChangeTracker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                saveCommissionProtocolCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}
