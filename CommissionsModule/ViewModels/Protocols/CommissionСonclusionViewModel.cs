using CommissionsModule.Services;
using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using Core.Extensions;
using Prism.Commands;

namespace CommissionsModule.ViewModels
{
    public class CommissionСonclusionViewModel : TrackableBindableBase
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ILog logService;

        private readonly Decision unselectedDecision;

        private CommandWrapper reInitializeCommandWrapper;

        private CancellationTokenSource currentOperationToken;
        #endregion

        #region Constructors

        public CommissionСonclusionViewModel(ICommissionService commissionService, ILog logService)
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

            Decisions = new ObservableCollectionEx<Decision>();

            reInitializeCommandWrapper = new CommandWrapper() { Command = new DelegateCommand(() => Initialize(CommissionProtocolId, PersonId)), CommandName = "Повторить" };
            unselectedDecision = new Decision { Name = "Выберите решение" };

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
        }

        #endregion

        #region Properties

        private DateTime protocolDate;
        public DateTime ProtocolDate
        {
            get { return protocolDate; }
            set { SetTrackedProperty(ref protocolDate, value); }
        }

        private int? protocolNumber;
        public int? ProtocolNumber
        {
            get { return protocolNumber; }
            set { SetTrackedProperty(ref protocolNumber, value); }
        }

        private string waitingFor;
        public string WaitingFor
        {
            get { return waitingFor; }
            set
            {
                SetTrackedProperty(ref waitingFor, value);
                NeedWaitingFor = !string.IsNullOrEmpty(waitingFor);
            }
        }

        private bool needWaitingFor;
        public bool NeedWaitingFor
        {
            get { return needWaitingFor; }
            set
            {
                SetProperty(ref needWaitingFor, value);
                if (!needWaitingFor && !string.IsNullOrEmpty(waitingFor))
                    WaitingFor = string.Empty;
            }
        }

        private string diagnosis;
        public string Diagnosis
        {
            get { return diagnosis; }
            set { SetTrackedProperty(ref diagnosis, value); }
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

        private DateTime? toDoDateTime = null;
        public DateTime? ToDoDateTime
        {
            get { return toDoDateTime; }
            set
            {
                SetTrackedProperty(ref toDoDateTime, value);
                NeedToDoDateTime = toDoDateTime != null;
            }
        }

        private bool needToDoDateTime;
        public bool NeedToDoDateTime
        {
            get { return needToDoDateTime; }
            set
            {
                SetProperty(ref needToDoDateTime, value);
                if (!needToDoDateTime && ToDoDateTime != null)
                    ToDoDateTime = null;
                else if (needToDoDateTime && ToDoDateTime == null)
                    ToDoDateTime = DateTime.Now.Date;
            }
        }

        public int CommissionProtocolId { get; private set; }
        public int PersonId { get; private set; }

        public ObservableCollectionEx<Decision> Decisions { get; set; }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        public IChangeTracker ChangeTracker { get; set; }
        #endregion

        #region Methods
        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionProtocolId = commissionProtocolId;
            PersonId = personId;
            ProtocolDate = DateTime.Now;
            ProtocolNumber = null;
            WaitingFor = string.Empty;
            Diagnosis = string.Empty;
            SelectedDecision = unselectedDecision;
            Comment = string.Empty;
            ToDoDateTime = null;

            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка протокола комиссии...");
            logService.InfoFormat("Loading commission protocol conclusion with id ={0} for person with id={1}", commissionProtocolId, personId);
            var commissionProtocolQuery = commissionService.GetCommissionProtocolById(commissionProtocolId);
            var curDate = DateTime.Now;
            try
            {
                var commissionProtocolData = await commissionProtocolQuery.Select(x => new
                {
                    x.Decision,
                    x.ProtocolDate,
                    x.ProtocolNumber,
                    x.Comment,
                    x.WaitingFor,
                    x.Diagnos,
                    ToDoDateTime,
                    x.PersonId,
                    ActualDateTime = x.ProtocolDate,
                    x.CommissionQuestionId
                }).FirstOrDefaultAsync(token);
                if (commissionProtocolData != null)
                {
                    PersonId = commissionProtocolData.PersonId;
                    curDate = commissionProtocolData.ActualDateTime;
                }
                var res = await LoadDecisions(curDate, commissionProtocolData.CommissionQuestionId, token);
                if (!res)
                {
                    logService.ErrorFormatEx(null, "Failed to load commission data sources with commission id ={0} for person with id={1}", commissionProtocolId, personId);
                    FailureMediator.Activate("Не удалость загрузить решения. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper);
                    return;
                }

                if (commissionProtocolData != null)
                {
                    ProtocolDate = commissionProtocolData.ProtocolDate;
                    ProtocolNumber = commissionProtocolData.ProtocolNumber > 0 ? commissionProtocolData.ProtocolNumber : (int?)null;
                    WaitingFor = commissionProtocolData.WaitingFor;
                    Diagnosis = commissionProtocolData.Diagnos;
                    SelectedDecision = commissionProtocolData.Decision;
                    Comment = commissionProtocolData.Comment;
                    ToDoDateTime = commissionProtocolData.ToDoDateTime;
                }
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission data sources with commission id ={0} for person with id={1}", commissionProtocolId, personId);
                FailureMediator.Activate("Не удалость загрузить протокол комиссии. Попробуйте еще раз или обратитесь в службу поддержки", reInitializeCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                    BusyMediator.Deactivate();
                if (commissionProtocolQuery != null)
                    commissionProtocolQuery.Dispose();
            }





        }

        public async Task<bool> LoadDecisions(DateTime onDate, int commissionQuestionId, CancellationToken token)
        {
            Decisions.Clear();
            logService.InfoFormat("Loading decisions for commissionQuestionIdwith id={0} on date={1}", commissionQuestionId, onDate);
            var dataloaded = false;
            try
            {
                var decisionsList = await Task.Factory.StartNew<IEnumerable<Decision>>(commissionService.GetDecisions,
                    new Tuple<int, int, DateTime>(commissionQuestionId, SpecialValues.NonExistingId, onDate));
                var decisions = new[] { unselectedDecision }.Concat(decisionsList).ToArray();
                logService.InfoFormat("Loaded {0} decisions", (decisions as Decision[]).Length);
                if (!token.IsCancellationRequested)
                    Decisions.AddRange(decisions);
                dataloaded = true;
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources  for PreliminaryProtocol for commission protocol with id={0}", CommissionProtocolId);
                dataloaded = false;
            }
            return dataloaded;
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

        public void GetСonclusionCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {

        }
        #endregion
    }
}
