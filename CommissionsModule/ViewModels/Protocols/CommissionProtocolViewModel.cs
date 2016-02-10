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

namespace CommissionsModule.ViewModels
{
    public class CommissionProtocolViewModel : BindableBase, IDisposable
    {
        #region Fields

        private readonly ICommissionService commissionService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogServiceAsync dialogService;
        private readonly ILog logService;

        private readonly Func<PersonSearchDialogViewModel> relativeSearchFactory;

        private CancellationTokenSource currentOperationToken;
        #endregion

        #region Constructiors
        public CommissionProtocolViewModel(ICommissionService commissionService, IEventAggregator eventAggregator, IDialogServiceAsync dialogService, ILog logService, PreliminaryProtocolViewModel preliminaryProtocolViewModel, CommissionСonductViewModel commissionСonductViewModel, CommissionСonclusionViewModel commissionСonclusionViewModel,
             Func<PersonSearchDialogViewModel> relativeSearchFactory)
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
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            this.commissionService = commissionService;
            this.logService = logService;
            this.dialogService = dialogService;
            this.relativeSearchFactory = relativeSearchFactory;
            this.eventAggregator = eventAggregator;
            PreliminaryProtocolViewModel = preliminaryProtocolViewModel;
            CommissionСonductViewModel = commissionСonductViewModel;
            CommissionСonclusionViewModel = commissionСonclusionViewModel;
            createCommissionCommand = new DelegateCommand(CreateCommission);
            addCommissionConductionCommand = new DelegateCommand<bool?>(AddCommissionConduction);
            addCommissionConclusionCommand = new DelegateCommand<bool?>(AddCommissionConclusion);
            SubscribeToEvents();

            FailureMediator = new FailureMediator();
            NotificationMediator = new NotificationMediator();
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
                    SetCommissionProtocolState();
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
                if (SelectedCommissionProtocolId == SpecialValues.NewId)
                {
                    PreliminaryProtocolViewModel.Initialize(personId: SelectedPersonId);
                    if ((int)CommissionProtocolState > 0)
                    {
                        CommissionСonductVisible = true;
                        CommissionСonductViewModel.Initialize(personId: SelectedPersonId);
                    }
                    if ((int)CommissionProtocolState > 1)
                    {
                        CommissionСonclusionVisible = true;
                        CommissionСonclusionViewModel.Initialize(personId: SelectedPersonId);
                    }
                }
                else
                {
                    PreliminaryProtocolViewModel.Initialize(SelectedCommissionProtocolId);
                    if ((int)CommissionProtocolState > 0)
                    {
                        CommissionСonductVisible = true;
                        CommissionСonductViewModel.Initialize(SelectedCommissionProtocolId);
                    }
                    if ((int)CommissionProtocolState > 1)
                    {
                        CommissionСonclusionVisible = true;
                        CommissionСonclusionViewModel.Initialize(SelectedCommissionProtocolId);
                    }
                }
            }
        }

        private string patient;
        public string Patient
        {
            get { return patient; }
            set { SetProperty(ref patient, value); }
        }

        public FailureMediator FailureMediator { get; set; }
        public NotificationMediator NotificationMediator { get; set; }
        #endregion

        #region Commands
        private DelegateCommand createCommissionCommand;
        public ICommand CreateCommissionCommand { get { return createCommissionCommand; } }

        private DelegateCommand<bool?> addCommissionConductionCommand;
        public ICommand AddCommissionConductionCommand { get { return addCommissionConductionCommand; } }

        private DelegateCommand<bool?> addCommissionConclusionCommand;
        public ICommand AddCommissionConclusionCommand { get { return addCommissionConclusionCommand; } }

        #endregion

        #region Methods

        private void AddCommissionConduction(bool? select)
        {
            if (select == null) return;
            if (select.Value)
                CommissionProtocolState = ViewModels.CommissionProtocolState.Сonduction;
            else
                CommissionProtocolState = ViewModels.CommissionProtocolState.Preliminary;
        }

        private void AddCommissionConclusion(bool? select)
        {
            if (select == null) return;
            if (select.Value)
                CommissionProtocolState = ViewModels.CommissionProtocolState.Сonclusion;
            else
                CommissionProtocolState = ViewModels.CommissionProtocolState.Сonduction;
        }

        private async void SetCommissionProtocolState()
        {
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
                            break;
                        case true:
                            CommissionProtocolState = CommissionProtocolState.Сonclusion;
                            break;
                        default:
                            CommissionProtocolState = CommissionProtocolState.Preliminary;
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
                FailureMediator.Activate("ННе удалось загрузить состояние протокола", exception: ex);
            }
            finally
            {
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
            }
        }

        private async void SetPatientName()
        {
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            logService.InfoFormat("Loading patient name with  person id ={0}", SelectedPersonId);
            var personQuery = commissionService.GetPerson(SelectedPersonId);
            try
            {
                var person = await personQuery.Select(x => new
                {
                    x.ShortName,
                    x.BirthDate
                }).FirstOrDefaultAsync(token);
                if (person != null)
                {
                    Patient = person.ShortName + person.BirthDate.ToShortDateString();
                }
                else
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
            }
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
            SelectedCommissionProtocolId = SpecialValues.NonExistingId;
            if (!SpecialValues.IsNewOrNonExisting(protocolId))
                SelectedCommissionProtocolId = protocolId;
        }

        #endregion
    }
}
