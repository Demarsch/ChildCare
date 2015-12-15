using Core.Data;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using System;

namespace PatientInfoModule.ViewModels
{
    public class ContractViewModel : BindableBase
    {        
        public ContractViewModel()
        { 
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private string contractName;
        public string ContractName
        {
            get { return contractName; }
            set { SetProperty(ref contractName, value); }
        }

        private string contractNumber;
        public string ContractNumber
        {
            get { return contractNumber; }
            set { SetProperty(ref contractNumber, value); }
        }

        private DateTime contractBeginDate;
        public DateTime ContractBeginDate
        {
            get { return contractBeginDate; }
            set { SetProperty(ref contractBeginDate, value); }
        }

        private DateTime contractEndDate;
        public DateTime ContractEndDate
        {
            get { return contractEndDate; }
            set { SetProperty(ref contractEndDate, value); }
        }

        private FieldValue client;
        public FieldValue Client
        {
            get { return client; }
            set { SetProperty(ref client, value); }
        }

        private string consumer;
        public string Consumer
        {
            get { return consumer; }
            set { SetProperty(ref consumer, value); }
        }

        private int financingSourceId;
        public int FinancingSourceId
        {
            get { return financingSourceId; }
            set { SetProperty(ref financingSourceId, value); }
        }

        private int paymentTypeId;
        public int PaymentTypeId
        {
            get { return paymentTypeId; }
            set { SetProperty(ref paymentTypeId, value); }
        }

        private int registratorId;
        public int RegistratorId
        {
            get { return registratorId; }
            set { SetProperty(ref registratorId, value); }
        }

        private bool isCashless;
        public bool IsCashless
        {
            get { return isCashless; }
            set { SetProperty(ref isCashless, value); }
        }

        private double contractCost;
        public double ContractCost
        {
            get { return contractCost; }
            set { SetProperty(ref contractCost, value); }
        }
    }
}
