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
        #endregion

        #region Constructors
        public CommissionJournalViewModel(ICommissionService commissionService, ILog logService, IDialogServiceAsync dialogService, IReportGeneratorHelper reportGenerator, Func<PersonSearchDialogViewModel> patientSearchFactory)
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
            this.dialogService = dialogService;
            this.commissionService = commissionService;
            this.logService = logService;
            this.reportGenerator = reportGenerator;
            this.patientSearchFactory = patientSearchFactory;

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            CommissionTypes = new ObservableCollectionEx<FieldValue>();
            VKItems = new ObservableCollectionEx<CommissionJournalItemViewModel>();

            searchPatientCommand = new DelegateCommand(SearchPatient);
            removePatientFilterCommand = new DelegateCommand(RemovePatientFilter);
            loadJournalCommand = new DelegateCommand(LoadJournal);

            printVKAssignmentCommand = new DelegateCommand(PrintVKAssignment);
            printVKProtocolCommand = new DelegateCommand(PrintVKProtocol);
            printVKJournalCommand = new DelegateCommand(PrintVKJournal);
        }

        #endregion

        #region Properties

        private readonly DelegateCommand loadJournalCommand;
        public ICommand LoadJournalCommand { get { return loadJournalCommand; } }

        private readonly DelegateCommand searchPatientCommand;
        public ICommand SearchPatientCommand { get { return searchPatientCommand; } }

        private readonly DelegateCommand removePatientFilterCommand;
        public ICommand RemovePatientFilterCommand { get { return removePatientFilterCommand; } }

        private readonly DelegateCommand printVKAssignmentCommand;
        public ICommand PrintVKAssignmentCommand { get { return printVKAssignmentCommand; } }

        private readonly DelegateCommand printVKProtocolCommand;
        public ICommand PrintVKProtocolCommand { get { return printVKProtocolCommand; } }

        private readonly DelegateCommand printVKJournalCommand;
        public ICommand PrintVKJournalCommand { get { return printVKJournalCommand; } }

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

        private ObservableCollectionEx<FieldValue> commissionTypes;
        public ObservableCollectionEx<FieldValue> CommissionTypes
        {
            get { return commissionTypes; }
            set { SetProperty(ref commissionTypes, value); }
        }

        private int selectedCommissionTypeId;
        public int SelectedCommissionTypeId
        {
            get { return selectedCommissionTypeId; }
            set { SetProperty(ref selectedCommissionTypeId, value); }
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

        private ObservableCollectionEx<CommissionJournalItemViewModel> vkItems;
        public ObservableCollectionEx<CommissionJournalItemViewModel> VKItems
        {
            get { return vkItems; }
            set { SetProperty(ref vkItems, value); }
        }

        private CommissionJournalItemViewModel selectedVK;
        public CommissionJournalItemViewModel SelectedVK
        {
            get { return selectedVK; }
            set { SetProperty(ref selectedVK, value); }
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
                CommissionTypes.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- выберите подкомиссию ВК -" });
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
            
            var result = commissionService.GetCommissionProtocols(selectedPatientId, beginDate, endDate, selectedCommissionTypeId, commissionNumberFilter, protocolNumberFilter);

            var query = await Task.Factory.StartNew(() =>
            {
                return result.Select(x => new
                {
                    Id = x.Id,
                    CommissionNumber = x.CommissionNumber,
                    ProtocolNumber = x.ProtocolNumber,
                    ProtocolDate = x.CommissionDate,
                    AssignPerson = x.User.Person.ShortName,
                    PatientFIO = x.Person.FullName,
                    PatientBirthDate = x.Person.BirthDate,
                    CardNumber = x.Person.AmbNumberString != string.Empty ? x.Person.AmbNumberString : "??",
                    PatientGender = x.Person.IsMale ? "муж" : "жен",
                    PatientSocialStatus = x.Person.PersonSocialStatuses.Select(a => new { a.SocialStatusType.Name, OrgName = a.OrgId.HasValue ? a.Org.Name : string.Empty, a.Office }),
                    PatientDiagnos = x.MKB,
                    CommissionType = x.CommissionType.Name,
                    CommissionName = x.CommissionQuestion.Name,
                    Decision = x.Decision.Name,
                    Recommendations = "??",
                    Details = "??",
                    Experts = x.CommissionType.CommissionMembers.Where(a => a.BeginDateTime <= endDate && a.EndDateTime >= beginDate && a.PersonStaffId.HasValue).Select(a => a.CommissionMemberType.Name + ": " + a.PersonStaff.Person.ShortName)
                })
                .ToArray();
            }, token);

            CommissionJournalItemViewModel[] journalItems = query.Select(x => new CommissionJournalItemViewModel()
                {
                    Id = x.Id,
                    CommissionNumber = x.CommissionNumber,
                    ProtocolNumber = x.ProtocolNumber,
                    ProtocolDate = x.ProtocolDate.ToShortDateString(),
                    AssignPerson = x.AssignPerson,
                    PatientFIO = x.PatientFIO,
                    PatientBirthDate = x.PatientBirthDate.ToShortDateString(),
                    CardNumber = x.CardNumber,
                    PatientGender = x.PatientGender,
                    PatientSocialStatus = x.PatientSocialStatus.Select(a => a.Name + " " + a.Office + " " + a.OrgName).Aggregate((a, b) => a + "\r\n" + b),
                    PatientDiagnos = x.PatientDiagnos,
                    CommissionType = x.CommissionType,
                    CommissionName = x.CommissionName,
                    Decision = x.Decision,
                    Recommendations = x.Recommendations,
                    Details = x.Details,
                    Experts = x.Experts != null ? x.Experts.Aggregate((a, b) => a + "\r\n" + b) : string.Empty
                }).ToArray();

            VKItems.Clear();
            VKItems.AddRange(journalItems);
        }

        private void PrintVKAssignment()
        {

        }

        private void PrintVKProtocol()
        {

        }

        private void PrintVKJournal()
        {            
            var report = reportGenerator.CreateDocX("CommissionsJournal");
            string emptyValue = string.Empty;
            string nonExistValue = "отсутствует";
            report.Data["OrgName"] = commissionService.GetDBSettingValue(DBSetting.OrgName);

            /*report.Data.Tables["hospdata"].AddRow(emptyValue, emptyValue, emptyValue);
            report.Data.Tables["radiationdata"].AddRow(emptyValue, emptyValue, emptyValue);*/
            report.Editable = false;
            report.Show();
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
