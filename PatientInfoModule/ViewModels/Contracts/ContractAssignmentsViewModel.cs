using Prism.Mvvm;
using System;

namespace PatientInfoModule.ViewModels
{
    public class ContractAssignmentsViewModel : BindableBase
    {
        public ContractAssignmentsViewModel()
        { 
        }

        public int Id { get; set; }

        private int recordTypeId;
        public int RecordTypeId
        {
            get { return recordTypeId; }
            set { SetProperty(ref recordTypeId, value); }
        }

        private string financingSourceName;
        public string FinancingSourceName
        {
            get { return financingSourceName; }
            set { SetProperty(ref financingSourceName, value); }
        }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { SetProperty(ref recordTypeName, value); }
        }

        private DateTime assignDateTime;
        public DateTime AssignDateTime
        {
            get { return assignDateTime; }
            set { SetProperty(ref assignDateTime, value); }
        }
               
        private double recordTypeCost;
        public double RecordTypeCost
        {
            get { return recordTypeCost; }
            set { SetProperty(ref recordTypeCost, value); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }
    }
}
