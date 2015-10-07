using System;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using DataLib;

namespace Core
{
    public class ContractItemDTO : ObservableObject
    {
        public int Id { get; set; }

        private bool isPaid;
        public bool IsPaid
        {
            get { return isPaid; }
            set { Set("IsPaid", ref isPaid, value); }
        }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { Set("RecordTypeName", ref recordTypeName, value); }
        }

        private int recordCount;
        public int RecordCount
        {
            get { return recordCount; }
            set { Set("RecordCount", ref recordCount, value); }
        }

        private double recordCost;
        public double RecordCost
        {
            get { return recordCost; }
            set { Set("RecordCost", ref recordCost, value); }
        }

        private ObservableCollection<PaymentType> paymentTypes;
        public ObservableCollection<PaymentType> PaymentTypes
        {
            get { return paymentTypes; }
            set { Set("PaymentTypes", ref paymentTypes, value); }
        }

        private PaymentType selectedPaymentType;
        public PaymentType SelectedPaymentType
        {
            get { return selectedPaymentType; }
            set { Set("SelectedPaymentType", ref selectedPaymentType, value); }
        }
    }
}
