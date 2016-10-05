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
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Regions;
using System.Windows.Input;
using System.Collections.ObjectModel;
using CommissionsModule.Services;
using Core.Wpf.Misc;
using Shared.Patient.ViewModels;
using Core.Reports;
using Core.Reports.DTO;

namespace CommissionsModule.ViewModels
{
    public class CommissionJournalViewModel : BindableBase, INavigationAware, IDisposable
    {
        #region Fields
        private readonly ICommissionService commissionService;
        private readonly IDialogServiceAsync dialogService;
        private readonly IReportGeneratorHelper reportGenerator;
        private readonly ILog logService;        
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        private CancellationTokenSource currentLoadingToken;
        private TaskCompletionSource<bool> initialLoadingTaskSource;
        private readonly CommandWrapper initialLoadingCommandWrapper;
        private readonly Func<PersonSearchDialogViewModel> patientSearchFactory;
        private readonly Func<PrintedDocumentsCollectionViewModel> printedDocumentsCollectionFactory;
        #endregion

        #region Constructors
        public CommissionJournalViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IReportGeneratorHelper reportGenerator,
            Func<PersonSearchDialogViewModel> patientSearchFactory, Func<PrintedDocumentsCollectionViewModel> printedDocumentsCollectionFactory)
        {
            if (commissionService == null)
            {
                throw new ArgumentNullException("commissionService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }            
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (patientSearchFactory == null)
            {
                throw new ArgumentNullException("patientSearchFactory");
            }
            if (reportGenerator == null)
            {
                throw new ArgumentNullException("reportGenerator");
            }
            if (printedDocumentsCollectionFactory == null)
            {
                throw new ArgumentNullException("printedDocumentsCollectionFactory");
            }
            this.dialogService = dialogService;
            this.commissionService = commissionService;
            this.logService = logService;
            this.reportGenerator = reportGenerator;
            this.patientSearchFactory = patientSearchFactory;
            this.printedDocumentsCollectionFactory = printedDocumentsCollectionFactory;

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            CommissionTypes = new ObservableCollectionEx<FieldValue>();
            CommissionQuestions = new ObservableCollectionEx<FieldValue>();
            CommissionItems = new ObservableCollectionEx<CommissionJournalItemViewModel>();

            searchPatientCommand = new DelegateCommand(SearchPatient);
            removePatientFilterCommand = new DelegateCommand(RemovePatientFilter);
            loadJournalCommand = new DelegateCommand(LoadJournal);

            printCommissionAssignmentCommand = new DelegateCommand(PrintCommissionAssignment);
            printCommissionProtocolCommand = new DelegateCommand(PrintCommissionProtocol);
            printCommissionJournalCommand = new DelegateCommand(PrintCommissionJournal);
        }

        #endregion

        #region Properties

        private readonly DelegateCommand loadJournalCommand;
        public ICommand LoadJournalCommand { get { return loadJournalCommand; } }

        private readonly DelegateCommand searchPatientCommand;
        public ICommand SearchPatientCommand { get { return searchPatientCommand; } }

        private readonly DelegateCommand removePatientFilterCommand;
        public ICommand RemovePatientFilterCommand { get { return removePatientFilterCommand; } }

        private readonly DelegateCommand printCommissionAssignmentCommand;
        public ICommand PrintCommissionAssignmentCommand { get { return printCommissionAssignmentCommand; } }

        private readonly DelegateCommand printCommissionProtocolCommand;
        public ICommand PrintCommissionProtocolCommand { get { return printCommissionProtocolCommand; } }

        private readonly DelegateCommand printCommissionJournalCommand;
        public ICommand PrintCommissionJournalCommand { get { return printCommissionJournalCommand; } }

        #region Filters

        private DateTime beginDate;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set { SetProperty(ref beginDate, value); }
        }

        private DateTime endDate;
        public DateTime EndDate
        {
            get { return endDate; }
            set { SetProperty(ref endDate, value); }
        }

        public ObservableCollectionEx<FieldValue> CommissionTypes { get; set; }

        private int selectedCommissionTypeId;
        public int SelectedCommissionTypeId
        {
            get { return selectedCommissionTypeId; }
            set
            { 
                if (SetProperty(ref selectedCommissionTypeId, value))
                {
                    var commissionQuestionsQuery = commissionService.GetCommissionQuestions(beginDate, endDate, selectedCommissionTypeId);
                    var commissionSelectQuestionsQuery = commissionQuestionsQuery.Select(x => new { x.Id, x.Name }).ToArray();
                    CommissionQuestions.Clear();
                    CommissionQuestions.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите вопрос комиссии -" });
                    CommissionQuestions.AddRange(commissionSelectQuestionsQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                    SelectedCommissionQuestionId = SpecialValues.NonExistingId;
                }            
            }
        }

        public ObservableCollectionEx<FieldValue> CommissionQuestions { get; set; }

        private int selectedCommissionQuestionId;
        public int SelectedCommissionQuestionId
        {
            get { return selectedCommissionQuestionId; }
            set { selectedCommissionQuestionId = 0; SetProperty(ref selectedCommissionQuestionId, value); }
        }

        private int selectedPatientId;
        public int SelectedPatientId
        {
            get { return selectedPatientId; }
            set 
            {
                SetProperty(ref selectedPatientId, value);
                SelectedPatient = (value != SpecialValues.NonExistingId ? commissionService.GetPerson(value).First().FullName : "все пациенты");
                IsPatientFilterSelected = (value != SpecialValues.NonExistingId);
            }
        }

        private string selectedPatient;
        public string SelectedPatient
        {
            get { return selectedPatient; }
            set { SetProperty(ref selectedPatient, value); }
        }

        private bool isPatientFilterSelected;
        public bool IsPatientFilterSelected
        {
            get { return isPatientFilterSelected; }
            set { SetProperty(ref isPatientFilterSelected, value); }
        }

        private string commissionNumberFilter;
        public string CommissionNumberFilter
        {
            get { return commissionNumberFilter; }
            set { SetProperty(ref commissionNumberFilter, value); }
        }

        private string protocolNumberFilter;
        public string ProtocolNumberFilter
        {
            get { return protocolNumberFilter; }
            set { SetProperty(ref protocolNumberFilter, value); }
        }

        #endregion

        private ObservableCollectionEx<CommissionJournalItemViewModel> commissionItems;
        public ObservableCollectionEx<CommissionJournalItemViewModel> CommissionItems
        {
            get { return commissionItems; }
            set { SetProperty(ref commissionItems, value); }
        }

        private CommissionJournalItemViewModel selectedCommissionItem;
        public CommissionJournalItemViewModel SelectedCommissionItem
        {
            get { return selectedCommissionItem; }
            set { SetProperty(ref selectedCommissionItem, value); }
        }

        #endregion

        #region Methods

        private async Task<bool> InitialLoadingAsync()
        {
            if (initialLoadingTaskSource != null)
            {
                return await initialLoadingTaskSource.Task;
            }
            initialLoadingTaskSource = new TaskCompletionSource<bool>();
            logService.Info("Loading data source for commissions journal...");
            FailureMediator.Deactivate();
            BusyMediator.Activate("Загрузка общих данных...");
            IDisposableQueryable<CommissionType> commissionTypesQuery = null;
           
            try
            {
                BeginDate = DateTime.Now;
                EndDate = beginDate;

                commissionTypesQuery = commissionService.GetCommissionTypes(beginDate, endDate);
                var commissionSelectTypesQuery = await commissionTypesQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                CommissionTypes.Clear();
                CommissionTypes.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите тип комиссии -" });
                CommissionTypes.AddRange(commissionSelectTypesQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                SelectedCommissionTypeId = SpecialValues.NonExistingId;
                
                SelectedPatientId = SpecialValues.NonExistingId;
                initialLoadingTaskSource.SetResult(true);
                return true;
            }
            catch (Exception ex)
            {
                logService.Error("Failed to load datasources for commission journal from database", ex);
                FailureMediator.Activate("При попытке загрузить список подкомиссий и услуг возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки",
                                         initialLoadingCommandWrapper, ex);
                initialLoadingTaskSource.SetResult(false);
                return false;
            }
            finally
            {
                BusyMediator.Deactivate();
                if (commissionTypesQuery != null)
                    commissionTypesQuery.Dispose();
            }
        }       

        private async void SearchPatient()
        {
            using (var searchViewModel = patientSearchFactory())
            {
                var result = await dialogService.ShowDialogAsync(searchViewModel);
                if (result != true)
                    return;
                SelectedPatientId = searchViewModel.PersonSearchViewModel.SelectedPersonId;
            }
        }

        private void RemovePatientFilter()
        {
            SelectedPatientId = SpecialValues.NonExistingId;
        }

        private async void LoadJournal()
        {
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            
            var result = commissionService.GetCommissionProtocols(selectedPatientId, beginDate, endDate, selectedCommissionTypeId, selectedCommissionQuestionId, commissionNumberFilter, protocolNumberFilter);

            var query = await Task.Factory.StartNew(() =>
            {
                return result.Select(x => new
                {
                    Id = x.Id,
                    PersonId = x.PersonId,
                    CommissionNumber = x.CommissionNumber,
                    ProtocolNumber = x.ProtocolNumber,
                    CommissionDate = x.CommissionDate,
                    AssignPerson = x.User.Person.ShortName,
                    PatientFIO = x.Person.FullName,
                    PatientBirthDate = x.Person.BirthDate,
                    CardNumber = x.Person.AmbNumberString != string.Empty ? "А/К №" + x.Person.AmbNumberString : "И/Б № ??",
                    BranchName = "??",
                    PatientGender = x.Person.IsMale ? "муж" : "жен",
                    PatientSocialStatus = x.Person.PersonSocialStatuses.Select(a => new { a.SocialStatusType.Name, OrgName = a.OrgId.HasValue ? a.Org.Name : string.Empty, a.Office }),
                    PatientDiagnos = x.MKB,
                    CommissionGroup = x.CommissionType.CommissionTypeGroup.Options,
                    CommissionTypeId = x.CommissionTypeId,
                    CommissionType = x.CommissionType.Name,
                    CommissionQuestionId = x.CommissionQuestionId,
                    CommissionName = x.CommissionQuestion.Name,
                    Decision = (x.Decision != null ? x.Decision.Name : "не рассмотрено"),
                    Recommendations = "??",
                    Details = "??",
                    Experts = x.CommissionType.CommissionMembers.Where(a => a.BeginDateTime <= endDate && a.EndDateTime >= beginDate && a.PersonStaffId.HasValue).Select(a => a.CommissionMemberType.Name + ": " + a.PersonStaff.Person.ShortName)
                })
                .ToArray();
            }, token);

            CommissionJournalItemViewModel[] journalItems = query.Select(x => new CommissionJournalItemViewModel()
                {
                    Id = x.Id,
                    PersonId = x.PersonId,
                    CommissionNumber = x.CommissionNumber,
                    ProtocolNumber = x.ProtocolNumber,
                    CommissionDate = x.CommissionDate.ToShortDateString(),
                    AssignPerson = x.AssignPerson,
                    PatientFIO = x.PatientFIO,
                    PatientBirthDate = x.PatientBirthDate.ToShortDateString(),
                    CardNumber = x.CardNumber,
                    BranchName = x.BranchName,
                    PatientGender = x.PatientGender,
                    PatientSocialStatus = x.PatientSocialStatus.Any() ? x.PatientSocialStatus.Select(a => a.Name + " " + a.Office + " " + a.OrgName).Aggregate((a, b) => a + "\r\n" + b) : string.Empty,
                    PatientDiagnos = x.PatientDiagnos,
                    CommissionGroup = x.CommissionGroup,
                    CommissionTypeId = x.CommissionTypeId,
                    CommissionType = x.CommissionType,
                    CommissionQuestionId = x.CommissionQuestionId,
                    CommissionName = x.CommissionName,
                    Decision = x.Decision,
                    Recommendations = x.Recommendations,
                    Details = x.Details,
                    Experts = x.Experts != null && x.Experts.Any() ? x.Experts.Aggregate((a, b) => a + "\r\n" + b) : string.Empty
                }).ToArray();

            CommissionItems.Clear();
            CommissionItems.AddRange(journalItems);

            if (CommissionItems.Any())
                SelectedCommissionItem = CommissionItems.First();
        }

        private void PrintCommissionAssignment()
        {
            if (SelectedCommissionItem == null)
                return;
            CommissionJournalDTO item = new CommissionJournalDTO()
            {
                Id = SelectedCommissionItem.Id,
                PersonId = SelectedCommissionItem.PersonId,
                CommissionNumber = SelectedCommissionItem.CommissionNumber,
                ProtocolNumber = SelectedCommissionItem.ProtocolNumber,
                CommissionDate = SelectedCommissionItem.CommissionDate,
                AssignPerson = SelectedCommissionItem.AssignPerson,
                PatientFIO = SelectedCommissionItem.PatientFIO,
                PatientBirthDate = SelectedCommissionItem.PatientBirthDate,
                CardNumber = SelectedCommissionItem.CardNumber,
                BranchName = SelectedCommissionItem.BranchName,
                PatientGender = SelectedCommissionItem.PatientGender,
                PatientSocialStatus = SelectedCommissionItem.PatientSocialStatus,
                PatientDiagnos = SelectedCommissionItem.PatientDiagnos,
                CommissionGroup = SelectedCommissionItem.CommissionGroup,
                CommissionTypeId = SelectedCommissionItem.CommissionTypeId,
                CommissionType = SelectedCommissionItem.CommissionType,
                CommissionQuestionId = SelectedCommissionItem.CommissionQuestionId,
                CommissionName = SelectedCommissionItem.CommissionName,
                Decision = SelectedCommissionItem.Decision,
                Recommendations = SelectedCommissionItem.Recommendations,
                Details = SelectedCommissionItem.Details,
                Experts = SelectedCommissionItem.Experts
            };

            if (SelectedCommissionItem.CommissionGroup == OptionValues.HospitalisationCommission)
                printedDocumentsCollectionFactory().LoadPrintedDocumentsAsync(OptionValues.ReferralToHospitalisationCommission, new FieldValue() { Field = "PersonId", Value = item.PersonId }, item);
            else
                printedDocumentsCollectionFactory().LoadPrintedDocumentsAsync(OptionValues.ReferralToCommonCommission, new FieldValue() { Field = "PersonId", Value = item.PersonId }, item);
        }

        private void PrintCommissionProtocol()
        {
            if (SelectedCommissionItem == null)
                return;
            var commissionQuestion = commissionService.GetCommissionQuestionById(SelectedCommissionItem.CommissionQuestionId).First();
            if (commissionQuestion.PrintedDocumentId.HasValue)
            {
                CommissionJournalDTO item = new CommissionJournalDTO()
                {
                    Id = SelectedCommissionItem.Id,
                    PersonId = SelectedCommissionItem.PersonId,
                    CommissionNumber = SelectedCommissionItem.CommissionNumber,
                    ProtocolNumber = SelectedCommissionItem.ProtocolNumber,
                    CommissionDate = SelectedCommissionItem.CommissionDate,
                    AssignPerson = SelectedCommissionItem.AssignPerson,
                    PatientFIO = SelectedCommissionItem.PatientFIO,
                    PatientBirthDate = SelectedCommissionItem.PatientBirthDate,
                    CardNumber = SelectedCommissionItem.CardNumber,
                    BranchName = SelectedCommissionItem.BranchName,
                    PatientGender = SelectedCommissionItem.PatientGender,
                    PatientSocialStatus = SelectedCommissionItem.PatientSocialStatus,
                    PatientDiagnos = SelectedCommissionItem.PatientDiagnos,
                    CommissionGroup = SelectedCommissionItem.CommissionGroup,
                    CommissionTypeId = SelectedCommissionItem.CommissionTypeId,
                    CommissionType = SelectedCommissionItem.CommissionType,
                    CommissionQuestionId = SelectedCommissionItem.CommissionQuestionId,
                    CommissionName = SelectedCommissionItem.CommissionName,
                    Decision = SelectedCommissionItem.Decision,
                    Recommendations = SelectedCommissionItem.Recommendations,
                    Details = SelectedCommissionItem.Details,
                    Experts = SelectedCommissionItem.Experts
                };
                printedDocumentsCollectionFactory().LoadPrintedDocumentsAsync(commissionQuestion.PrintedDocumentId.Value, new FieldValue() { Field = "PersonId", Value = item.PersonId }, item);
            }
        }

        private void PrintCommissionJournal()
        {
            if (SelectedCommissionTypeId == SpecialValues.NonExistingId)
            {
                FailureMediator.Activate("Не выбран тип комиссии. Печать журнала невозможна.", true);
                return;
            }
            var commissionType = commissionService.GetCommissionTypeById(SelectedCommissionTypeId).First();
            if (commissionType.PrintedDocumentId.HasValue)
            {
                CommissionJournalDTO[] items = CommissionItems.Select(x => new CommissionJournalDTO()
                    {
                        Id = x.Id,
                        PersonId = SelectedCommissionItem.PersonId,
                        CommissionNumber = x.CommissionNumber,
                        ProtocolNumber = x.ProtocolNumber,
                        CommissionDate = x.CommissionDate,
                        AssignPerson = x.AssignPerson,
                        PatientFIO = x.PatientFIO,
                        PatientBirthDate = x.PatientBirthDate,
                        CardNumber = x.CardNumber,
                        BranchName = x.BranchName,
                        PatientGender = x.PatientGender,
                        PatientSocialStatus = x.PatientSocialStatus,
                        PatientDiagnos = x.PatientDiagnos,
                        CommissionGroup = x.CommissionGroup,
                        CommissionTypeId = x.CommissionTypeId,
                        CommissionType = x.CommissionType,
                        CommissionQuestionId = x.CommissionQuestionId,
                        CommissionName = x.CommissionName,
                        Decision = x.Decision,
                        Recommendations = x.Recommendations,
                        Details = x.Recommendations,
                        Experts = x.Experts
                    }).ToArray();
                printedDocumentsCollectionFactory().LoadPrintedDocumentsAsync(commissionType.PrintedDocumentId.Value, null, items);
            }
        }

        public void Dispose()
        {
            
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

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await InitialLoadingAsync();
        }
        #endregion
    }
}
