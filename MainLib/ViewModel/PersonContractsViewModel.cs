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
using System.Windows.Threading;

namespace MainLib.ViewModel
{
    public class PersonContractsViewModel : ObservableObject
    {
        private ILog log;
        private IDialogService dialogService;
        private IPersonService personService;
        private IRecordService recordService;
        private IAssignmentService assignmentService;
        private int? personId;
        private Dispatcher dispatcher;

        public PersonContractsViewModel(IPersonService personService, IRecordService recordService, IAssignmentService assignmentService, IDialogService dialogService, ILog log)
        {            
            this.personService = personService;
            this.dialogService = dialogService;
            this.recordService = recordService;
            this.assignmentService = assignmentService;
            this.log = log;
            this.dispatcher = Dispatcher.CurrentDispatcher;

            this.AddContractCommand = new RelayCommand(AddContract);
            this.RemoveContractCommand = new RelayCommand(RemoveContract);
            this.PrintContractsCommand = new RelayCommand(PrintContracts);                     
        }

        public void Load(int? personId = null)
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
        }

        private void LoadContracts()
        {          
            var contractsQuery = personService.GetContracts(this.personId, null, null, SelectedRegistratorId).OrderBy(x => x.BeginDateTime);
            ContractsCount = contractsQuery.Count().ToSafeString();
            ContractsSum = (contractsQuery.Any() ? personService.GetContractCost(contractsQuery.Select(x => x.Id).ToArray()) : 0) + " руб.";
            Contracts = new ObservableCollection<ContractsViewModel>();
            dispatcher.InvokeAsync(new Action(()=>
            {
                foreach (var contract in contractsQuery)
                    Contracts.Add(new ContractsViewModel(this.personId.Value, personService, recordService, assignmentService, dialogService, log)
                    {
                        Id = contract.Id,
                        ContractNumber = contract.Number.ToSafeString(),
                        Client = personService.GetPersonById(contract.ClientId.Value).ShortName,
                        ContractCost = personService.GetContractCost(contract.Id).ToSafeString() + " руб.",
                        ContractDate = contract.BeginDateTime.ToShortDateString()
                    });
                if (Contracts.Any())
                    SelectedContract = Contracts.OrderByDescending(x => x.ContractBeginDateTime).First();
            }));   
        }

        private void AddContract()
        {
            if (Contracts.Any(x => x.Id == 0)) return;
            Contracts.Add(new ContractsViewModel(this.personId.Value, personService, recordService, assignmentService, dialogService, log) 
            {
                Id = 0,
                ContractNumber = string.Empty,
                Client = "НОВЫЙ ДОГОВОР",
                ContractCost = string.Empty,
                ContractDate = DateTime.Now.ToShortDateString()
            });
            SelectedContract = Contracts.First(x => x.Id == 0);
        }

        private void RemoveContract()
        {
            if (this.dialogService.AskUser("Удалить договор " + SelectedContract.ContractName + "?", true) == true)
            {
                var visit = recordService.GetVisitsByContractId(SelectedContract.Id).FirstOrDefault();
                if (visit != null)
                {
                    this.dialogService.ShowMessage("Данный договор уже закреплен за случаем обращения пациента " + personService.GetPersonById(visit.PersonId).ShortName + ". Удаление договора невозможно.");
                    return;
                }
                personService.DeleteContractItems(SelectedContract.Id);
                personService.DeleteContract(SelectedContract.Id);
                Contracts.Remove(SelectedContract);
            }
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
            set
            {
                if (!Set(() => SelectedContract, ref selectedContract, value) || value == null)
                    return;
                selectedContract.LoadContract();
            }
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
            set 
            { 
                if (!Set(() => SelectedRegistratorId, ref selectedRegistratorId, value))
                    return;
                LoadContracts();
            }
        }
    }
}
