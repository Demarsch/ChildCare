using System;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using DataLib;

namespace Core
{
    public class ContractItemDTO : ObservableObject
    {
        public int Id { get; set; }

        private int? recordContractId;
        public int?  RecordContractId
        {
            get { return recordContractId; }
            set { Set(() => RecordContractId, ref recordContractId, value); }
        }

        private int? assignmentId;
        public int? AssignmentId
        {
            get { return assignmentId; }
            set { Set(() => AssignmentId, ref assignmentId, value); }
        }

        private int recordTypeId;
        public int RecordTypeId
        {
            get { return recordTypeId; }
            set { Set(() => RecordTypeId, ref recordTypeId, value); }
        }

        private bool isPaid;
        public bool IsPaid
        {
            get { return isPaid; }
            set { Set(() => IsPaid, ref isPaid, value); }
        }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { Set(() => RecordTypeName, ref recordTypeName, value); }
        }

        private int recordCount;
        public int RecordCount
        {
            get { return recordCount; }
            set { Set(() => RecordCount, ref recordCount, value); }
        }

        private double recordCost;
        public double RecordCost
        {
            get { return recordCost; }
            set { Set(() => RecordCost, ref recordCost, value); }
        }

        private int? appendix;
        public int? Appendix
        {
            get { return appendix; }
            set { Set(() => Appendix, ref appendix, value); }
        }
    }
}
