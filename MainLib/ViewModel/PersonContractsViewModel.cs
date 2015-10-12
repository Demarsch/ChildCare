using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using log4net;
using System.Windows.Threading;

namespace MainLib.ViewModel
{
    public class PersonContractsViewModel : ObservableObject
    {
        private ILog log;
        private IDialogService dialogService;
        private IPersonService personService;
        private IRecordService recordService;
        private int? personId;
        private Dispatcher dispatcher;

        public PersonContractsViewModel(IPersonService personService, IRecordService recordService, IDialogService dialogService, ILog log)
        {            
            this.personService = personService;
            this.dialogService = dialogService;
            this.recordService = recordService;
            this.log = log;
            this.dispatcher = Dispatcher.CurrentDispatcher;

            this.AddContractCommand = new RelayCommand(AddContract);
            this.RemoveContractCommand = new RelayCommand(RemoveContract);
            this.PrintContractsCommand = new RelayCommand(PrintContracts);                     
        }

        public async void Load(int? personId = null)
        {
            this.personId = personId;
            ShowPeriod = !this.personId.HasValue;
            var contractRecord = recordService.GetRecordTypesByOptions("|contract|").FirstOrDefault();
            var reliableStaff = recordService.GetRecordTypeRolesByOptions("|responsible|contract|pay|").FirstOrDefault();
            if (contractRecord == null || reliableStaff == null) return;

            var personStaffs = personService.GetAllowedPersonStaffs(contractRecord.Id, reliableStaff.Id);
            if (!personStaffs.Any()) return;
            List<FieldValue> elements = new List<FieldValue>();
            elements.Add(new FieldValue() { Value = -1, Field = "- все -" });
            elements.AddRange(personStaffs.Select(x => new FieldValue() { Value = x.Id, Field = personService.GetPersonById(x.PersonId).ShortName }));
            Registrators = new ObservableCollection<FieldValue>(elements);
            SelectedRegistratorId = -1;

            LoadContracts();
        }

        private void LoadContracts()
        {          
            var contractsQuery = personService.GetContracts(this.personId, null, null, SelectedRegistratorId);
            ContractsCount = contractsQuery.Count().ToSafeString();
            ContractsSum = personService.GetContractCost(contractsQuery.Select(x => x.Id).ToArray()) + " руб.";
            contracts = new ObservableCollection<ContractsViewModel>();
            dispatcher.InvokeAsync(new Action(()=>
            {
                foreach (var contract in contractsQuery)
                contracts.Add(new ContractsViewModel(contract.Id, personService, recordService, dialogService, log)
                {
                    ContractNumber = contract.Number.ToSafeString(),
                    Client = personService.GetPersonById(contract.ConsumerId.Value).ShortName,
                    Cost = personService.GetContractCost(contract.Id).ToSafeString() + " руб."
                });
            }));   
        }

        private void AddContract()
        {
            
        }

        private void RemoveContract()
        {

        }

        private void PrintContracts()
        {

        }

        private RelayCommand addContractCommand;
        public RelayCommand AddContractCommand
        {
            get { return addContractCommand; }
            set { Set(() => AddContractCommand, ref addContractCommand, value); }
        }

        private RelayCommand removeContractCommand;
        public RelayCommand RemoveContractCommand
        {
            get { return removeContractCommand; }
            set { Set(() => RemoveContractCommand, ref removeContractCommand, value); }
        }

        private RelayCommand printContractsCommand;
        public RelayCommand PrintContractsCommand
        {
            get { return printContractsCommand; }
            set { Set(() => PrintContractsCommand, ref printContractsCommand, value); }
        }

        private ObservableCollection<ContractsViewModel> contracts;
        public ObservableCollection<ContractsViewModel> Contracts
        {
            get { return contracts; }
            set { Set(() => Contracts, ref contracts, value); }
        }

        private ContractsViewModel selectedContract;
        public ContractsViewModel SelectedContract
        {
            get { return selectedContract; }
            set { Set(() => SelectedContract, ref selectedContract, value); }
        }

        private bool showPeriod;
        public bool ShowPeriod
        {
            get { return showPeriod; }
            set { Set(() => ShowPeriod, ref showPeriod, value); }
        }

        private string contractsCount;
        public string ContractsCount
        {
            get { return contractsCount; }
            set { Set(() => ContractsCount, ref contractsCount, value); }
        }

        private string contractsSum;
        public string ContractsSum
        {
            get { return contractsSum; }
            set { Set(() => ContractsSum, ref contractsSum, value); }
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
    }
}
