using Core.Wpf.Events;
using OrganizationContractsModule.Services;
using Prism.Mvvm;
using System;
using System.Drawing;
using System.Windows;

namespace OrganizationContractsModule.ViewModels
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

        private string orgDetails;
        public string OrgDetails
        {
            get { return orgDetails; }
            set { SetProperty(ref orgDetails, value); }
        }

        private int orgId;
        public int OrgId
        {
            get { return orgId; }
            set { SetProperty(ref orgId, value); }
        }
    }
}
