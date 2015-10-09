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
        private int contractId;

        public ContractsViewModel(int contractId, IPersonService personService, IRecordService recordService, IDialogService dialogService, ILog log)
        {            
            this.personService = personService;
            this.dialogService = dialogService;
            this.recordService = recordService;
            this.log = log;
            this.contractId = contractId;

            /*var contract = personService.GetContractById(contractId);
            ContractNumber = contract.Number.ToSafeString();
            Client = personService.GetPersonById(contract.ConsumerId.Value).ShortName;
            Cost = personService.GetContractCost(contractId).ToSafeString() + " руб.";*/

            PersonSuggestionProvider = new PersonSuggestionProvider(personService);
            RecordTypesSuggestionProvider = new RecordTypesSuggestionProvider(recordService);

            this.AddRecordCommand = new RelayCommand(AddRecord);
            this.RemoveRecordCommand = new RelayCommand(RemoveRecord);
            this.AddAppendixCommand = new RelayCommand(AddAppendix);
            this.RemoveAppendixCommand = new RelayCommand(RemoveAppendix);

            this.SaveContractCommand = new RelayCommand(SaveContract);
            this.PrintContractCommand = new RelayCommand(PrintContract);
            this.PrintAppendixCommand = new RelayCommand(PrintAppendix);
        }
               
        private void AddRecord()
        {

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

        private string client;
        public string Client
        {
            get { return client; }
            set { Set(() => Client, ref client, value); }
        }

        private string cost;
        public string Cost
        {
            get { return cost; }
            set { Set(() => Cost, ref cost, value); }
        }

        private ObservableCollection<FinancingSource> financingSources;
        public ObservableCollection<FinancingSource> FinancingSources
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

        private ObservableCollection<PersonStaff> registrators;
        public ObservableCollection<PersonStaff> Registrators
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
            set { Set(() => SelectedRecord, ref selectedRecord, value); }
        }

        private bool isAssignRecordsChecked;
        public bool IsAssignRecordsChecked
        {
            get { return isAssignRecordsChecked; }
            set { Set(() => IsAssignRecordsChecked, ref isAssignRecordsChecked, value); }
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
    }
}
