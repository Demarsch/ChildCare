using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.PopupWindowActionAware;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using Core.Extensions;
using Core.Wpf.Misc;
using Core.Misc;
using PatientInfoModule.Services;
using PatientInfoModule.Misc;

namespace PatientInfoModule.ViewModels
{
    public class AddContractRecordsViewModel : BindableBase, INotification, IPopupWindowActionAware, IDataErrorInfo
    {
        private readonly IRecordService recordService;
        private readonly IAssignmentService assignmentService;
        private readonly IPatientService personService;
        private readonly ILog logService;
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; private set; }
        private DateTime contractDate = SpecialValues.MinDate;
        private bool isChild = false;
        private int personId;

        public AddContractRecordsViewModel(IRecordService recordService, IAssignmentService assignmentService, IPatientService personService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (assignmentService == null)
            {
                throw new ArgumentNullException("assignmentService");
            }
            if (personService == null)
            {
                throw new ArgumentNullException("personService");
            }
            this.recordService = recordService;
            this.assignmentService = assignmentService;
            this.personService = personService;
            this.logService = logService;
            RecordTypesSuggestionProvider = new RecordTypesSuggestionProvider(recordService);
            Assignments = new ObservableCollectionEx<ContractAssignmentsViewModel>();

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            SelectRecordsCommand = new DelegateCommand(SelectRecords);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public void IntializeCreation(string title, int personId, int financingSourceId, DateTime contractDate, bool isNewRecordChecked = false, bool isAssignRecordsChecked = false)
        {  
            this.Title = title;
            this.personId = personId;
            this.contractDate = contractDate;
            this.isChild = personService.GetPatientQuery(personId).First<Person>().BirthDate.Date.AddYears(18) >= contractDate.Date;
            IsAssignRecordsChecked = isAssignRecordsChecked;
            IsNewRecordChecked = isNewRecordChecked;
            FinancingSources = recordService.GetActiveFinancingSources();
            SelectedFinancingSourceId = financingSourceId;   
        }

        private bool canSelectRecords;
        public bool CanSelectRecords
        {
            get { return canSelectRecords; }
            set { SetProperty(ref canSelectRecords, value); }
        }

        private bool isNewRecordChecked;
        public bool IsNewRecordChecked
        {
            get { return isNewRecordChecked; }
            set 
            {
                if (SetProperty(ref isNewRecordChecked, value) && value)
                    CanSelectRecords = true;
            }
        }

        private bool isAssignRecordsChecked;
        public bool IsAssignRecordsChecked
        {
            get { return isAssignRecordsChecked; }
            set 
            { 
                if (SetProperty(ref isAssignRecordsChecked, value) && value)
                    CanSelectRecords = true;
            }
        }

        private IEnumerable<FinancingSource> financingSources;
        public IEnumerable<FinancingSource> FinancingSources
        {
            get { return financingSources; }
            set { SetProperty(ref financingSources, value); }
        }

        private int selectedFinancingSourceId;
        public int SelectedFinancingSourceId
        {
            get { return selectedFinancingSourceId; }
            set 
            {
                if (SetProperty(ref selectedFinancingSourceId, value) && value != SpecialValues.NonExistingId)
                {
                    if (selectedRecord != null && isNewRecordChecked)
                        AssignRecordTypeCost = (recordService.GetRecordTypeCost(selectedRecord.Id, selectedFinancingSourceId, contractDate, isChild) * recordsCount);
                    Assignments = new ObservableCollectionEx<ContractAssignmentsViewModel>(assignmentService.GetPersonAssignments(personId)
                                            .Where(x => x.FinancingSourceId == selectedFinancingSourceId && !x.RecordContractItems.Any()).ToList()
                                            .Select(x => new ContractAssignmentsViewModel()
                                            {
                                                Id = x.Id,
                                                RecordTypeId = x.RecordTypeId,
                                                AssignDateTime = x.AssignDateTime,
                                                RecordTypeName = x.RecordType.Name,
                                                RecordTypeCost = recordService.GetRecordTypeCost(x.RecordTypeId, selectedFinancingSourceId, contractDate, isChild)
                                            }));
                }
            }
        }

        private double assignRecordTypeCost;
        public double AssignRecordTypeCost
        {
            get { return assignRecordTypeCost; }
            set { SetProperty(ref assignRecordTypeCost, value); }
        }

        private int recordsCount;
        public int RecordsCount
        {
            get { return recordsCount; }
            set
            {
                SetProperty(ref recordsCount, value);
                AssignRecordTypeCost = (selectedRecord != null ? (recordService.GetRecordTypeCost(selectedRecord.Id, selectedFinancingSourceId, contractDate, isChild) * recordsCount) : 0.0);
            }
        }

        private RecordTypesSuggestionProvider recordTypesSuggestionProvider;
        public RecordTypesSuggestionProvider RecordTypesSuggestionProvider
        {
            get { return recordTypesSuggestionProvider; }
            set { SetProperty(ref recordTypesSuggestionProvider, value); }
        }

        private RecordType selectedRecord;
        public RecordType SelectedRecord
        {
            get { return selectedRecord; }
            set
            {
                SetProperty(ref selectedRecord, value);                
                if (selectedFinancingSourceId == -1)
                    FailureMediator.Activate("Укажите источник финансирования.", null, null, true);
                else
                    RecordsCount = 1;
            }
        }

        private ObservableCollectionEx<ContractAssignmentsViewModel> assignments;
        public ObservableCollectionEx<ContractAssignmentsViewModel> Assignments
        {
            get { return assignments; }
            set { SetProperty(ref assignments, value); }
        }

        private ContractAssignmentsViewModel selectedAssignment;
        public ContractAssignmentsViewModel SelectedAssignment
        {
            get { return selectedAssignment; }
            set { SetProperty(ref selectedAssignment, value); }
        }

        public ICommand CancelCommand { get; private set; }
        private void Cancel()
        {
            isOk = false;
            HostWindow.Close();            
        }

        public bool isOk = false;
        public ICommand SelectRecordsCommand { get; private set; }
        private void SelectRecords()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                FailureMediator.Activate("Проверьте правильность заполнения полей.", null, null, true);
                return;
            }
            isOk = true;
            HostWindow.Close();
        }              

        #region IPopupWindowActionAware implementation
        public System.Windows.Window HostWindow { get; set; }

        public INotification HostNotification { get; set; }
        #endregion

        #region INotification implementation
        public object Content { get; set; }

        public string Title { get; set; }
        #endregion

        #region IDataErrorInfo implementation
        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                switch (columnName)
                {
                    case "SelectedFinancingSourceId":
                        result = SelectedFinancingSourceId == SpecialValues.NonExistingId ? "Укажите источник финансирования" : string.Empty;
                        break;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
