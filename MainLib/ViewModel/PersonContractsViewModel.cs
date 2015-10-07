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
    public class PersonContractsViewModel : ObservableObject
    {
        private ILog log;
        private IDialogService dialogService;
        private IPersonService personService;
        private int? personId;

        public PersonContractsViewModel(IPersonService personService, IDialogService dialogService, ILog log)
        {            
            this.personService = personService;
            this.dialogService = dialogService;
            this.log = log;

            this.AddContractCommand = new RelayCommand(AddContract);
            this.RemoveContractCommand = new RelayCommand(RemoveContract);
            this.PrintContractsCommand = new RelayCommand(PrintContracts);

            //Registrators = pers
        }

        public async void Load(int? personId = null)
        {
            this.personId = personId;
            await LoadContracts();
        }

        private async Task LoadContracts()
        {   

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
            set { Set("AddContractCommand", ref addContractCommand, value); }
        }

        private RelayCommand removeContractCommand;
        public RelayCommand RemoveContractCommand
        {
            get { return removeContractCommand; }
            set { Set("RemoveContractCommand", ref removeContractCommand, value); }
        }

        private RelayCommand printContractsCommand;
        public RelayCommand PrintContractsCommand
        {
            get { return printContractsCommand; }
            set { Set("PrintContractsCommand", ref printContractsCommand, value); }
        }

        private ObservableCollection<ContractsViewModel> contracts;
        public ObservableCollection<ContractsViewModel> Contracts
        {
            get { return contracts; }
            set { Set("Contracts", ref contracts, value); }
        }

        private ContractsViewModel selectedContract;
        public ContractsViewModel SelectedContract
        {
            get { return selectedContract; }
            set { Set("SelectedContract", ref selectedContract, value); }
        }

        private bool showPeriod;
        public bool ShowPeriod
        {
            get { return showPeriod; }
            set { Set("ShowPeriod", ref showPeriod, value); }
        }

        private int contractsCount;
        public int ContractsCount
        {
            get { return contractsCount; }
            set { Set("ContractsCount", ref contractsCount, value); }
        }

        private double contractsSum;
        public double ContractsSum
        {
            get { return contractsSum; }
            set { Set("ContractsSum", ref contractsSum, value); }
        }

        private ObservableCollection<PersonStaff> registrators;
        public ObservableCollection<PersonStaff> Registrators
        {
            get { return registrators; }
            set { Set("Registrators", ref registrators, value); }
        }

        private PersonStaff selectedRegistrator;
        public PersonStaff SelectedRegistrator
        {
            get { return selectedRegistrator; }
            set { Set("SelectedRegistrator", ref selectedRegistrator, value); }
        }
    }
}
