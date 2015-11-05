using Prism.Mvvm;

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

        private string contractBeginDate;
        public string ContractBeginDate
        {
            get { return contractBeginDate; }
            set { SetProperty(ref contractBeginDate, value); }
        }

        private string contractEndDate;
        public string ContractEndDate
        {
            get { return contractEndDate; }
            set { SetProperty(ref contractEndDate, value); }
        }

        private string client;
        public string Client
        {
            get { return client; }
            set { SetProperty(ref client, value); }
        }

        private double contractCost;
        public double ContractCost
        {
            get { return contractCost; }
            set { SetProperty(ref contractCost, value); }
        }
    }
}
