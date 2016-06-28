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
using System.ComponentModel;

namespace CommissionsModule.ViewModels
{
    public class CommissionСonclusionViewModel : TrackableBindableBase, IChangeTrackerMediator, IDataErrorInfo
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ILog logService;

        private readonly Decision unselectedDecision;

        private CommandWrapper reInitializeCommandWrapper;

        private CancellationTokenSource currentOperationToken;

        private ValidationMediator validationMediator;
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
            CompositeChangeTracker = new ChangeTrackerEx<CommissionСonclusionViewModel>(this);
            validationMediator = new ValidationMediator(this);
        }

        #endregion

        #region Properties

        private DateTime commissionDate;
        public DateTime CommissionDate
        {
            get { return commissionDate; }
            set { SetTrackedProperty(ref commissionDate, value); }
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
                NeedToDoDateTime = value != null;
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
        public IChangeTracker CompositeChangeTracker { get; set; }
        #endregion

        #region Methods
        public async void Initialize(int commissionProtocolId = SpecialValues.NonExistingId, int personId = SpecialValues.NonExistingId)
        {
            CompositeChangeTracker.IsEnabled = false;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            CommissionProtocolId = commissionProtocolId;
            PersonId = personId;
            CommissionDate = DateTime.Now;
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
                    x.CommissionDate,
                    x.ProtocolNumber,
                    x.Comment,
                    x.WaitingFor,
                    x.Diagnos,
                    x.ToDoDateTime,
                    x.PersonId,
                    ActualDateTime = x.CommissionDate,
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
                    CommissionDate = commissionProtocolData.CommissionDate;
                    ProtocolNumber = commissionProtocolData.ProtocolNumber > 0 ? commissionProtocolData.ProtocolNumber : (int?)null;
                    WaitingFor = commissionProtocolData.WaitingFor;
                    Diagnosis = commissionProtocolData.Diagnos;
                    SelectedDecision = commissionProtocolData.Decision;
                    Comment = commissionProtocolData.Comment;
                    ToDoDateTime = commissionProtocolData.ToDoDateTime;
                }
                loadingIsCompleted = true;
                CompositeChangeTracker.IsEnabled = true;
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

        public bool GetСonclusionCommissionProtocolData(ref CommissionProtocol commissionProtocol)
        {
            if (validationMediator.Validate())
            {
                if (commissionProtocol != null)
                {
                    commissionProtocol.ProtocolNumber = ProtocolNumber.ToInt();
                    commissionProtocol.CommissionDate = CommissionDate;
                    commissionProtocol.WaitingFor = WaitingFor;
                    commissionProtocol.Diagnos = Diagnosis;
                    commissionProtocol.DecisionId = SelectedDecision.Id;
                    commissionProtocol.Comment = Comment;
                    commissionProtocol.ToDoDateTime = ToDoDateTime;
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
        #endregion

        #region IDataErrorInfo implementation
        public string Error
        {
            get { return validationMediator.Error; }
        }

        public string this[string columnName]
        {
            get { return validationMediator[columnName]; }
        }

        public bool Validate()
        {
            return validationMediator.Validate();
        }

        public void CancelValidation()
        {
            validationMediator.CancelValidation();
        }

        public class ValidationMediator : ValidationMediator<CommissionСonclusionViewModel>
        {

            public ValidationMediator(CommissionСonclusionViewModel associatedItem)
                : base(associatedItem)
            {
            }

            protected override void OnValidateProperty(string propertyName)
            {
                if (PropertyNameEquals(propertyName, x => x.ProtocolNumber))
                {
                    ValidateProtocolNumber();
                }
                else if (PropertyNameEquals(propertyName, x => x.CommissionDate))
                {
                    ValidateProtocolDate();
                }
                else if (PropertyNameEquals(propertyName, x => x.WaitingFor))
                {
                    ValidateWaitingFor();
                }
                else if (PropertyNameEquals(propertyName, x => x.SelectedDecision))
                {
                    ValidateSelectedDecision();
                }
                else if (PropertyNameEquals(propertyName, x => x.ToDoDateTime))
                {
                    ValidateToDoDateTime();
                }
            }

            private void ValidateToDoDateTime()
            {
                SetError(x => x.ToDoDateTime, AssociatedItem.NeedToDoDateTime && AssociatedItem.ToDoDateTime == null ? "Укажите дату, когда предполагается выполнения решения" : string.Empty);
            }

            private void ValidateSelectedDecision()
            {
                SetError(x => x.SelectedDecision, AssociatedItem.SelectedDecision != null && SpecialValues.IsNewOrNonExisting(AssociatedItem.SelectedDecision.Id) ? "Укажите решение комиссии" : string.Empty);
            }

            private void ValidateWaitingFor()
            {
                SetError(x => x.WaitingFor, AssociatedItem.NeedWaitingFor && string.IsNullOrEmpty(AssociatedItem.WaitingFor) ? "Укажите условие, выполнение которого ожидает результат протокола" : string.Empty);
            }

            private void ValidateProtocolDate()
            {
                SetError(x => x.ProtocolNumber, AssociatedItem.ProtocolNumber.ToInt() < 1 ? "Укажите номер протокола" : string.Empty);
            }

            private void ValidateProtocolNumber()
            {
                SetError(x => x.CommissionDate, AssociatedItem.CommissionDate == null ? "Укажите дату комиссии" : string.Empty);
            }


            protected override void RaiseAssociatedObjectPropertyChanged()
            {
                AssociatedItem.OnPropertyChanged(string.Empty);
            }

            protected override void OnValidate()
            {
                ValidateProtocolNumber();
                ValidateProtocolDate();
                ValidateWaitingFor();
                ValidateSelectedDecision();
                ValidateToDoDateTime();
            }

        }

        #endregion
    }
}
