using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;
using MainLib.View;

namespace MainLib.ViewModel
{
    public class ContractsViewModel : ObservableObject
    {
        private ILog log;
        private IDialogService dialogService;
        private IPersonService personService;
        private IRecordService recordService;
        private IAssignmentService assignmentService;
        private int personId;

        public ContractsViewModel(int personId, IPersonService personService, IRecordService recordService, IAssignmentService assignmentService, IDialogService dialogService, ILog log)
        {            
            this.personService = personService;
            this.dialogService = dialogService;
            this.recordService = recordService;
            this.assignmentService = assignmentService;
            this.log = log;
            this.personId = personId;

            this.AddRecordCommand = new RelayCommand(AddRecord);
            this.RemoveRecordCommand = new RelayCommand(RemoveRecord);
            this.AddAppendixCommand = new RelayCommand(AddAppendix);
            this.RemoveAppendixCommand = new RelayCommand(RemoveAppendix);

            this.SaveContractCommand = new RelayCommand(SaveContract);
            this.PrintContractCommand = new RelayCommand(PrintContract);
            this.PrintAppendixCommand = new RelayCommand(PrintAppendix);
        }

        internal void LoadContract()
        {
            PersonSuggestionProvider = new PersonSuggestionProvider(personService);
            RecordTypesSuggestionProvider = new RecordTypesSuggestionProvider(recordService);

            var contractRecord = recordService.GetRecordTypesByOptions("|contract|").FirstOrDefault();
            var reliableStaff = recordService.GetRecordTypeRolesByOptions("|responsible|contract|pay|").FirstOrDefault();
            if (contractRecord == null || reliableStaff == null) return;

            var personStaffs = personService.GetAllowedPersonStaffs(contractRecord.Id, reliableStaff.Id);
            if (!personStaffs.Any()) return;
            List<FieldValue> elements = new List<FieldValue>();
            elements.Add(new FieldValue() { Value = -1, Field = "- выберите ответственного -" });
            elements.AddRange(personStaffs.Select(x => new FieldValue() { Value = x.Id, Field = personService.GetPersonById(x.PersonId).ShortName }));
            Registrators = new ObservableCollection<FieldValue>(elements);
            
            List<FieldValue> finSources = new List<FieldValue>();
            finSources.Add(new FieldValue() { Value = -1, Field = "- выберите ист. финансирования -" });
            finSources.AddRange(recordService.GetActiveFinancingSources().Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            FinancingSources = new ObservableCollection<FieldValue>(finSources);

            List<FieldValue> paymentTypesSource = new List<FieldValue>();
            paymentTypesSource.Add(new FieldValue() { Value = -1, Field = "- выберите метод оплаты -" });
            paymentTypesSource.AddRange(recordService.GetPaymentTypes().Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            PaymentTypes = new ObservableCollection<FieldValue>(paymentTypesSource);

            SelectedFinancingSourceId = -1;
            SelectedRegistratorId = -1;
            SelectedPaymentTypeId = -1;
            IsCashless = false;
            ContractItems = new ObservableCollection<ContractItemDTO>();
            Assignments = new ObservableCollection<AssignmentDTO>();

            SelectedClient = personService.GetPersonById(Id != 0 ? personService.GetContractById(Id).ClientId.Value : personId);
            SelectedConsumer = personService.GetPersonById(Id != 0 ? personService.GetContractById(Id).ConsumerId.Value : personId);
            Assignments = new ObservableCollection<AssignmentDTO>(assignmentService.GetAssignments(Id != 0 ? personService.GetContractById(Id).ConsumerId.Value : personId)
                                            .Select(x => new AssignmentDTO()
                                            {
                                                Id = x.Id,
                                                RecordTypeId = x.RecordTypeId,
                                                AssignDateTime = x.AssignDateTime,
                                                RecordTypeName = recordService.GetRecordTypeById(x.RecordTypeId).Name,
                                                RecordTypeCost = recordService.GetRecordTypeCost(x.RecordTypeId)
                                            }));

            if (Id != 0)
            {
                var contract = personService.GetContractById(Id);
                ContractBeginDateTime = contract.BeginDateTime;
                ContractEndDateTime = contract.EndDateTime;
                ContractName = contract.DisplayName;
                SelectedFinancingSourceId = contract.FinancingSourceId;
                SelectedRegistratorId = contract.InUserId;
                SelectedPaymentTypeId = contract.PaymentTypeId;
                ContractItems = new ObservableCollection<ContractItemDTO>(personService.GetContractItems(Id)
                                                .Select(x => new ContractItemDTO()
                                                {
                                                    Id = x.Id,
                                                    RecordContractId = x.RecordContractId,
                                                    AssignmentId = x.AssignmentId,
                                                    RecordTypeId = x.RecordTypeId,                                                    
                                                    IsPaid = x.IsPaid,
                                                    RecordTypeName = recordService.GetRecordTypeById(x.RecordTypeId).Name,
                                                    RecordCount = x.Count,
                                                    RecordCost = x.Cost,
                                                    Appendix = x.Appendix
                                                }));
            }
            else
            {
                ContractBeginDateTime = DateTime.Now;
                ContractEndDateTime = DateTime.Now;
                ContractName = "НОВЫЙ ДОГОВОР";                   
            }
        }

        private void AddRecord()
        {
            int? appendix = null;
            if (ContractItems.Any(x => x.Appendix.HasValue))
                appendix = ContractItems.Where(x => x.Appendix.HasValue).Select(x => x.Appendix.Value).OrderByDescending(x => x).First();
            if (isAssignRecordsChecked)
            {
                foreach (var assignment in assignments.Where(x => x.IsSelected))
                {
                    ContractItems.Add(new ContractItemDTO()
                    {
                        Id = 0,
                        RecordContractId = (int?)null,
                        AssignmentId = assignment.Id,
                        RecordTypeId = assignment.RecordTypeId,                        
                        IsPaid = true,
                        RecordTypeName = assignment.RecordTypeName,
                        RecordCount = 1,
                        RecordCost = assignment.RecordTypeCost,
                        Appendix = appendix
                    });
                }
            }
            else
            {
                ContractItems.Add(new ContractItemDTO()
                {
                    Id = 0,
                    RecordContractId = (int?)null,
                    AssignmentId = (int?)null,
                    RecordTypeId = SelectedRecord.Id,                    
                    IsPaid = true,
                    RecordTypeName = SelectedRecord.Name,
                    RecordCount = RecordsCount,
                    RecordCost = AssignRecordTypeCost,
                    Appendix = appendix
                });
            }
        }

        private void RemoveRecord()
        {

        }

        private void AddAppendix()
        {

        }

        private void RemoveAppendix()
        {

        }

        private void SaveContract()
        {
            if (AllowSave())
            {
                RecordContract contract = new RecordContract();
                if (Id != 0)
                    contract = personService.GetContractById(Id);
                if (!contract.Number.HasValue)
                {
                    DateTime beginYear = new DateTime(ContractBeginDateTime.Year, 1, 1);
                    DateTime endYear = new DateTime(ContractBeginDateTime.Year, 12, 31);
                    contract.Number = personService.GetContracts(null, beginYear, endYear).Select(x => x.Number.Value).FirstUnused();
                }
                contract.BeginDateTime = ContractBeginDateTime;
                contract.EndDateTime = ContractEndDateTime;
                contract.FinancingSourceId = SelectedFinancingSourceId;
                contract.ClientId = SelectedClient.Id;
                contract.ConsumerId = SelectedConsumer.Id;
                contract.ContractName = SelectedClient.ShortName;
                contract.PaymentTypeId = SelectedPaymentTypeId;
                if (recordService.GetPaymentTypeById(SelectedPaymentTypeId).Options.Contains("|cashless|"))
                {
                    contract.TransactionNumber = TransationNumber;
                    contract.TransactionDate = TransationDate;
                }
                contract.Priority = 1;
                contract.InUserId = SelectedRegistratorId;
                contract.InDateTime = DateTime.Now;
                contract.OrgId = (int?)null;
                contract.ContractCost = 0;
                contract.Options = string.Empty;

                string message = string.Empty;
                contract.Id = personService.SaveContractData(contract, out message);
                if (contract.Id == 0)               
                {
                    dialogService.ShowError("При сохранении договора возникла ошибка: " + message);
                    log.Error(string.Format("Failed to Save RecordContract. " + message));
                    return;
                }
                Id = contract.Id;

                foreach (ContractItemDTO itemDTO in ContractItems)
                {
                    RecordContractItem item = personService.GetContractItemById(itemDTO.Id);
                    if (item == null)
                        item = new RecordContractItem();
                    item.RecordContractId = contract.Id;
                    item.AssignmentId = itemDTO.AssignmentId;
                    item.RecordTypeId = itemDTO.RecordTypeId;                                                    
                    item.IsPaid = itemDTO.IsPaid;
                    item.Count = itemDTO.RecordCount;
                    item.Cost = itemDTO.RecordCost;
                    item.Appendix = itemDTO.Appendix;
                    item.InUserId = contract.InUserId;
                    item.InDateTime = contract.InDateTime;
                    item.Id = personService.SaveContractItemData(item, out message);
                    if (item.Id == 0)
                    {
                        dialogService.ShowError("При сохранении услуг по договору возникла ошибка: " + message);
                        log.Error(string.Format("Failed to Save RecordContractItem. " + message));
                        return;
                    }
                    itemDTO.Id = item.Id;
                }

                ContractNumber = contract.Number.ToSafeString();
                Client = contract.ContractName;
                ContractCost = personService.GetContractCost(contract.Id).ToSafeString() + " руб.";
                ContractDate = contract.BeginDateTime.ToShortDateString();
                
                dialogService.ShowMessage("Данные сохранены");
            }
        }

        private bool AllowSave()
        {
            if (SelectedFinancingSourceId == -1)
            {
                dialogService.ShowMessage("Не выбран источник финансирования");
                return false;
            }
            if (SelectedRegistratorId == -1)
            {
                dialogService.ShowMessage("Не выбран ответственный за договор");
                return false;
            }
            if (SelectedPaymentTypeId == -1)
            {
                dialogService.ShowMessage("Не выбран метод оплаты");
                return false;
            }
            if (SelectedClient == null)
            {
                dialogService.ShowMessage("Не выбран заказчик");
                return false;
            }
            if (SelectedConsumer == null)
            {
                dialogService.ShowMessage("Не выбран потребитель");
                return false;
            }
            if (!ContractItems.Any())
            {
                dialogService.ShowMessage("Отсутствуют услуги");
                return false;
            }
            return true;
        }

        private void PrintContract()
        {

        }

        private void PrintAppendix()
        {

        }

        private RelayCommand addRecordCommand;
        public RelayCommand AddRecordCommand
        {
            get { return addRecordCommand; }
            set { Set(() => AddRecordCommand, ref addRecordCommand, value); }
        }

        private RelayCommand removeRecordCommand;
        public RelayCommand RemoveRecordCommand
        {
            get { return removeRecordCommand; }
            set { Set(() => RemoveRecordCommand, ref removeRecordCommand, value); }
        }

        private RelayCommand addAppendixCommand;
        public RelayCommand AddAppendixCommand
        {
            get { return addAppendixCommand; }
            set { Set(() => AddAppendixCommand, ref addAppendixCommand, value); }
        }

        private RelayCommand removeAppendixCommand;
        public RelayCommand RemoveAppendixCommand
        {
            get { return removeAppendixCommand; }
            set { Set(() => RemoveAppendixCommand, ref removeAppendixCommand, value); }
        }

        private RelayCommand saveContractCommand;
        public RelayCommand SaveContractCommand
        {
            get { return saveContractCommand; }
            set { Set(() => SaveContractCommand, ref saveContractCommand, value); }
        }

        private RelayCommand printContractCommand;
        public RelayCommand PrintContractCommand
        {
            get { return printContractCommand; }
            set { Set(() => PrintContractCommand, ref printContractCommand, value); }
        }

        private RelayCommand printAppendixCommand;
        public RelayCommand PrintAppendixCommand
        {
            get { return printAppendixCommand; }
            set { Set(() => PrintAppendixCommand, ref printAppendixCommand, value); }
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { Set(() => Id, ref id, value); }
        }

        private string contractName;
        public string ContractName
        {
            get { return contractName; }
            set { Set(() => ContractName, ref contractName, value); }
        }

        private string contractNumber;
        public string ContractNumber
        {
            get { return contractNumber; }
            set { Set(() => ContractNumber, ref contractNumber, value); }
        }

        private DateTime contractBeginDateTime;
        public DateTime ContractBeginDateTime
        {
            get { return contractBeginDateTime; }
            set { Set(() => ContractBeginDateTime, ref contractBeginDateTime, value); }
        }

        private DateTime contractEndDateTime;
        public DateTime ContractEndDateTime
        {
            get { return contractEndDateTime; }
            set { Set(() => ContractEndDateTime, ref contractEndDateTime, value); }
        }

        private string client;
        public string Client
        {
            get { return client; }
            set { Set(() => Client, ref client, value); }
        }

        private string contractCost;
        public string ContractCost
        {
            get { return contractCost; }
            set { Set(() => ContractCost, ref contractCost, value); }
        }

        private string contractDate;
        public string ContractDate
        {
            get { return contractDate; }
            set { Set(() => ContractDate, ref contractDate, value); }
        }

        private ObservableCollection<FieldValue> financingSources;
        public ObservableCollection<FieldValue> FinancingSources
        {
            get { return financingSources; }
            set { Set(() => FinancingSources, ref financingSources, value); }
        }

        private int selectedFinancingSourceId;
        public int SelectedFinancingSourceId
        {
            get { return selectedFinancingSourceId; }
            set { Set(() => SelectedFinancingSourceId, ref selectedFinancingSourceId, value); }
        }

        private ObservableCollection<FieldValue> registrators;
        public ObservableCollection<FieldValue> Registrators
        {
            get { return registrators; }
            set { Set(() => Registrators, ref registrators, value); }
        }

        private int selectedRegistratorId;
        public int SelectedRegistratorId
        {
            get { return selectedRegistratorId; }
            set { Set(() => SelectedRegistratorId, ref selectedRegistratorId, value); }
        }

        private ObservableCollection<FieldValue> paymentTypes;
        public ObservableCollection<FieldValue> PaymentTypes
        {
            get { return paymentTypes; }
            set { Set(() => PaymentTypes, ref paymentTypes, value); }
        }

        private int selectedPaymentTypeId;
        public int SelectedPaymentTypeId
        {
            get { return selectedPaymentTypeId; }
            set 
            { 
                if (!Set(() => SelectedPaymentTypeId, ref selectedPaymentTypeId, value))
                    return;
                IsCashless = (value != -1) && recordService.GetPaymentTypeById(value).Options.Contains("|cashless|");
            }
        }

        private bool isCashless;
        public bool IsCashless
        {
            get { return isCashless; }
            set { Set(() => IsCashless, ref isCashless, value); }
        }

        private PersonSuggestionProvider personSuggestionProvider;
        public PersonSuggestionProvider PersonSuggestionProvider
        {
            get { return personSuggestionProvider; }
            set { Set(() => PersonSuggestionProvider, ref personSuggestionProvider, value); }
        }

        private Person selectedClient;
        public Person SelectedClient
        {
            get { return selectedClient; }
            set { Set(() => SelectedClient, ref selectedClient, value); }
        }

        private Person selectedConsumer;
        public Person SelectedConsumer
        {
            get { return selectedConsumer; }
            set { Set(() => SelectedConsumer, ref selectedConsumer, value); }
        }

        private bool isNewRecordChecked;
        public bool IsNewRecordChecked
        {
            get { return isNewRecordChecked; }
            set { Set(() => IsNewRecordChecked, ref isNewRecordChecked, value); }
        }

        private bool isAssignRecordsChecked;
        public bool IsAssignRecordsChecked
        {
            get { return isAssignRecordsChecked; }
            set { Set(() => IsAssignRecordsChecked, ref isAssignRecordsChecked, value); }
        }
        
        private double assignRecordTypeCost;
        public double AssignRecordTypeCost
        {
            get { return assignRecordTypeCost; }
            set { Set(() => AssignRecordTypeCost, ref assignRecordTypeCost, value); }
        }

        private int recordsCount;
        public int RecordsCount
        {
            get { return recordsCount; }
            set
            {
                if (!Set(() => RecordsCount, ref recordsCount, value))
                    return;
                AssignRecordTypeCost = (recordService.GetRecordTypeCost(selectedRecord.Id) * recordsCount);
            }
        }

        private RecordTypesSuggestionProvider recordTypesSuggestionProvider;
        public RecordTypesSuggestionProvider RecordTypesSuggestionProvider
        {
            get { return recordTypesSuggestionProvider; }
            set { Set(() => RecordTypesSuggestionProvider, ref recordTypesSuggestionProvider, value); }
        }

        private RecordType selectedRecord;
        public RecordType SelectedRecord
        {
            get { return selectedRecord; }
            set 
            {
                if (!Set(() => SelectedRecord, ref selectedRecord, value) || value == null)
                    return;
                RecordsCount = 1;
            }
        }
                
        private ObservableCollection<AssignmentDTO> assignments;
        public ObservableCollection<AssignmentDTO> Assignments
        {
            get { return assignments; }
            set { Set(() => Assignments, ref assignments, value); }
        }

        private AssignmentDTO selectedAssignment;
        public AssignmentDTO SelectedAssignment
        {
            get { return selectedAssignment; }
            set { Set(() => SelectedAssignment, ref selectedAssignment, value); }
        }

        private ObservableCollection<ContractItemDTO> contractItems;
        public ObservableCollection<ContractItemDTO> ContractItems
        {
            get { return contractItems; }
            set { Set(() => ContractItems, ref contractItems, value); }
        }

        private ContractItemDTO selectedContractItem;
        public ContractItemDTO SelectedContractItem
        {
            get { return selectedContractItem; }
            set { Set(() => SelectedContractItem, ref selectedContractItem, value); }
        }

        private string transationNumber;
        public string TransationNumber
        {
            get { return transationNumber; }
            set { Set(() => TransationNumber, ref transationNumber, value); }
        }

        private string transationDate;
        public string TransationDate
        {
            get { return transationDate; }
            set { Set(() => TransationDate, ref transationDate, value); }
        }
    }
}
