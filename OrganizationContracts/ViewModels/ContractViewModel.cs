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

        private string contractNumber;
        public string ContractNumber
        {
            get { return contractNumber; }
            set { SetProperty(ref contractNumber, value); }
        }

        private string organizationName;
        public string OrganizationName
        {
            get { return organizationName; }
            set { SetProperty(ref organizationName, value); }
        }

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
    }
}
