using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Regions;
using System.Windows.Media;
using PatientInfoModule.Services;
using System.Windows.Input;
using Core.Services;
using Shared.Commissions.Events;
using Core.Reports.DTO;
using Core.Reports;

namespace PatientInfoModule.ViewModels
{
    public class PersonCommissionsCollectionViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly ILog logService;
        private readonly IDialogService messageService;
        private readonly ISecurityService securityService;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private readonly Func<PrintedDocumentsCollectionViewModel> printedDocumentsCollectionFactory;
        #endregion

        #region  Constructors
        public PersonCommissionsCollectionViewModel(ICommissionService commissionService, ILog logService, IDialogService messageService,
            ISecurityService securityService, IUserService userService, IEventAggregator eventAggregator, Func<PrintedDocumentsCollectionViewModel> printedDocumentsCollectionFactory)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (printedDocumentsCollectionFactory == null)
            {
                throw new ArgumentNullException("printedDocumentsCollectionFactory");
            }
            this.eventAggregator = eventAggregator;
            this.commissionService = commissionService;
            this.logService = logService;
            this.userService = userService;
            this.securityService = securityService;
            this.messageService = messageService;
            this.printedDocumentsCollectionFactory = printedDocumentsCollectionFactory;

            //removeCommissionProtocolCommand = new DelegateCommand<int?>(RemoveCommissionProtocol);
            printCommissionProtocolCommand = new DelegateCommand<int?>(PrintCommissionProtocol);

            Commissions = new ObservableCollectionEx<PersonCommissionViewModel>();
            BusyMediator = new BusyMediator();

            SubscribeToEvents();
        }
                
        #endregion

        #region Properties
        public BusyMediator BusyMediator { get; set; }

        //private DelegateCommand<int?> removeCommissionProtocolCommand;
        //public ICommand RemoveCommissionProtocolCommand { get { return removeCommissionProtocolCommand; } }

        private DelegateCommand<int?> printCommissionProtocolCommand;
        public ICommand PrintCommissionProtocolCommand { get { return printCommissionProtocolCommand; } }

        public ObservableCollectionEx<PersonCommissionViewModel> Commissions { get; set; }

        private PersonCommissionViewModel selectedCommission;
        public PersonCommissionViewModel SelectedCommission
        {
            get { return selectedCommission; }
            set 
            {
                if (SetProperty(ref selectedCommission, value) && value != null)
                {
                    eventAggregator.GetEvent<PubSubEvent<int>>().Publish(value.Id);
                    eventAggregator.GetEvent<PubSubEvent<PersonCommissionViewModel>>().Publish(value);
                }
            }
        }

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set
            {
                if (SetProperty(ref personId, value) && !SpecialValues.IsNewOrNonExisting(value))
                    LoadCommissionProtocols();
            }
        }

        private async void LoadCommissionProtocols()
        {
            await LoadCommissionProtocolsAsync();
        }

        #endregion

        #region Methods

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<CommissionChangedEvent>().Subscribe(OnCommissionsChanged);
        }

        private void UnsubscribeFromEvents()
        {
            eventAggregator.GetEvent<CommissionChangedEvent>().Unsubscribe(OnCommissionsChanged);
        }

        private async void OnCommissionsChanged(int commissionProtocolId)
        {
            await LoadCommissionProtocolsAsync();
        }

        public async Task LoadCommissionProtocolsAsync()
        {
            BusyMediator.Activate("Загрузка протоколов пациента...");
            logService.Info("Loading person commission protocols...");
            Commissions.Clear();
            IDisposableQueryable<CommissionProtocol> commissionProtocolsQuery = null;
            try
            {
                commissionProtocolsQuery = commissionService.GetPersonCommissionProtocols(PersonId);

                var commissionProtocolsSelectQuery = await commissionProtocolsQuery.Select(x => new
                    {
                        Id = x.Id,
                        PersonId = x.PersonId,
                        PatientFIO = x.Person.ShortName,
                        BirthDate = x.Person.BirthDate.Year,
                        Talon = x.PersonTalon != null ? new { TalonNumber = x.PersonTalon.TalonNumber, TalonDate = x.PersonTalon.TalonDateTime } : null,
                        MKB = x.MKB,
                        CommissionDate = x.CommissionDate,
                        IsCompleted = x.IsCompleted,
                        DecisionId = x.DecisionId,
                        Question = x.CommissionQuestion.ShortName
                    }).ToArrayAsync();

                var result = commissionProtocolsSelectQuery.Select(x => new PersonCommissionViewModel()
                    {
                        Id = x.Id,
                        PersonId = x.PersonId,
                        PatientFIO = x.PatientFIO,
                        BirthDate = x.BirthDate + " г.р.",
                        Talon = x.Talon != null ? x.Talon.TalonNumber + " от " + x.Talon.TalonDate.ToShortDateString() : "талон отсутствует",
                        MKB = !string.IsNullOrEmpty(x.MKB) ? "МКБ: " + x.MKB : string.Empty,
                        CommissionDate = x.CommissionDate.ToShortDateString(),
                        IsCompleted = x.IsCompleted,
                        DecisionText = x.DecisionId.HasValue ? commissionService.GetDecisionById(x.DecisionId.Value).Name : "не рассмотрено",
                        DecisionColorHex = commissionService.GetDecisionColorHex(x.DecisionId),
                        Question = x.Question
                    }).ToArray();

                Commissions.AddRange(result);
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load commission protocols");
                messageService.ShowError("Не удалось загрузить протоколы комиссий пациента. ");
            }
            finally
            {
                if (commissionProtocolsQuery != null)
                    commissionProtocolsQuery.Dispose();
                BusyMediator.Deactivate();
            }
        }

        /*private async void RemoveCommissionProtocol(int? selectedProtocolId)
        {
            if (SpecialValues.IsNewOrNonExisting(PersonId))
            {
                messageService.ShowWarning("Не выбран пациент.");
                return;
            }
            if (!selectedProtocolId.HasValue || SpecialValues.IsNewOrNonExisting(selectedProtocolId.Value))
            {
                messageService.ShowWarning("Выберите комиссию");
                return;
            }
            if (messageService.AskUser("Удалить комиссию ?") == true)
            {
                if (!securityService.HasPermission(Permission.RemoveCommissionProtocol))
                {
                    messageService.ShowWarning("У вас нет прав на удаление комиссии.");
                    return;
                }
                var deletedCommission = commissionService.GetCommissionProtocolById(selectedProtocolId.Value).FirstOrDefault();
                if (deletedCommission != null && deletedCommission.InUserId != userService.GetCurrentUser().Id)
                {
                    messageService.ShowWarning("Комиссию может удалить только тот, кто ее создал.");
                    return;
                }
                bool isOk = await commissionService.RemoveCommissionProtocol(deletedCommission.Id);
                if (isOk)
                    LoadCommissionProtocolsAsync();
            }
        }*/

        private void PrintCommissionProtocol(int? selectedProtocolId)
        {
            if (SpecialValues.IsNewOrNonExisting(PersonId))
            {
                messageService.ShowWarning("Не выбран пациент.");
                return;
            }
            if (!selectedProtocolId.HasValue || SpecialValues.IsNewOrNonExisting(selectedProtocolId.Value))
            {
                messageService.ShowWarning("Выберите комиссию");
                return;
            }
            var commissionProtocol = commissionService.GetCommissionProtocolById(selectedProtocolId.Value).First();
            if (commissionProtocol.CommissionQuestion.PrintedDocumentId.HasValue)
            {
                CommissionJournalDTO item = new CommissionJournalDTO()
                {
                    Id = commissionProtocol.Id,
                    PersonId = commissionProtocol.PersonId,
                    CommissionNumber = commissionProtocol.CommissionNumber,
                    ProtocolNumber = commissionProtocol.ProtocolNumber,
                    CommissionDate = commissionProtocol.CommissionDate.ToShortDateString(),
                    AssignPerson =  commissionProtocol.User.Person.ShortName,
                    PatientFIO = commissionProtocol.Person.FullName,
                    PatientBirthDate = commissionProtocol.Person.BirthDate.ToShortDateString(),
                    CardNumber = commissionProtocol.Person.AmbNumberString != string.Empty ? "А/К №" + commissionProtocol.Person.AmbNumberString : "И/Б № ??",
                    BranchName = "??",
                    PatientGender = commissionProtocol.Person.IsMale ? "муж" : "жен",
                    PatientSocialStatus = commissionProtocol.Person.PersonSocialStatuses.Any() ? 
                                            commissionProtocol.Person.PersonSocialStatuses
                                                            .Select(a => new { a.SocialStatusType.Name, OrgName = a.OrgId.HasValue ? a.Org.Name : string.Empty, a.Office })
                                                            .Select(a => a.Name + " " + a.Office + " " + a.OrgName).Aggregate((a, b) => a + "\r\n" + b) : string.Empty,
                    PatientDiagnos = commissionProtocol.Diagnos + "; " + commissionProtocol.MKB,
                    CommissionGroup = commissionProtocol.CommissionType.CommissionTypeGroup.Options,
                    CommissionTypeId = commissionProtocol.CommissionTypeId,
                    CommissionType = commissionProtocol.CommissionType.Name,
                    CommissionQuestionId = commissionProtocol.CommissionQuestionId,
                    CommissionName = commissionProtocol.CommissionQuestion.Name,
                    Decision = commissionProtocol.Decision != null ? commissionProtocol.Decision.Name : "на рассмотрении",
                    Recommendations = "??",
                    Details = "??",
                    Experts = commissionProtocol.CommissionType.CommissionMembers.Any(a => a.BeginDateTime <= commissionProtocol.CommissionDate && a.EndDateTime >= commissionProtocol.CommissionDate && a.PersonStaffId.HasValue) ?
                              commissionProtocol.CommissionType.CommissionMembers.Where(a => a.BeginDateTime <= commissionProtocol.CommissionDate && a.EndDateTime >= commissionProtocol.CommissionDate && a.PersonStaffId.HasValue)
                                                .Select(a => a.CommissionMemberType.Name + ": " + a.PersonStaff.Person.ShortName)
                                                .Aggregate((a, b) => a + "\r\n" + b) : string.Empty
                };
                printedDocumentsCollectionFactory().LoadPrintedDocumentsAsync(commissionProtocol.CommissionQuestion.PrintedDocumentId.Value, new FieldValue() { Field = "PersonId", Value = item.PersonId }, item);
            }
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }

        #endregion             
    }
}
