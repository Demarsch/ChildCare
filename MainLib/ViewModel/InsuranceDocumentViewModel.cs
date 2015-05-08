using DataLib;
using System;
using GalaSoft.MvvmLight;
using Core;
using log4net;

namespace MainLib
{
    public class InsuranceDocumentViewModel : ObservableObject
    {

        private readonly IPersonService service;

        private readonly InsuranceDocument insuranceDocument;

        public InsuranceDocumentViewModel(InsuranceDocument insuranceDocument, IPersonService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.insuranceDocument = insuranceDocument;
            FillData();
        }

        private void FillData()
        {
            if (!IsEmpty)
            {
                InsuranceCompanyId = insuranceDocument.InsuranceCompanyId;
                InsuranceDocumentTypeId = insuranceDocument.InsuranceDocumentTypeId;
                InsuranceCompany = service.GetInsuranceCompany(InsuranceCompanyId);
                Series = insuranceDocument.Series;
                Number = insuranceDocument.Number;
                BeginDate = insuranceDocument.BeginDate;
                EndDate = insuranceDocument.EndDate;
                WithoutEndDate = insuranceDocument.EndDate.Date == DateTime.MaxValue.Date;
            }
            else
            {
                InsuranceCompanyId = 0;
                InsuranceDocumentTypeId = 0;
                InsuranceCompany = null;
                Series = string.Empty;
                Number = string.Empty;
                BeginDate = new DateTime(1900, 1, 1);
                EndDate = DateTime.MaxValue.Date;
            }
        }

        public bool IsEmpty
        {
            get { return insuranceDocument == null; }
        }

        private int insuranceCompanyId = 0;
        public int InsuranceCompanyId
        {
            get { return insuranceCompanyId; }
            set { Set("InsuranceCompanyId", ref insuranceCompanyId, value); }
        }

        private InsuranceCompany insuranceCompany;
        public InsuranceCompany InsuranceCompany
        {
            get { return insuranceCompany; }
            set { Set("InsuranceCompany", ref insuranceCompany, value); }
        }

        private int insuranceDocumentTypeId = 0;
        public int InsuranceDocumentTypeId
        {
            get { return insuranceDocumentTypeId; }
            set { Set("InsuranceDocumentTypeId", ref insuranceDocumentTypeId, value); }
        }

        private string series = string.Empty;
        public string Series
        {
            get { return series; }
            set { Set("Series", ref series, value); }
        }

        private string number = string.Empty;
        public string Number
        {
            get { return number; }
            set { Set("Number", ref number, value); }
        }

        private DateTime beginDate = DateTime.MinValue;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set
            {
                Set("BeginDate", ref beginDate, value);
                RaisePropertyChanged("PersonInsuranceDocumentState");
                RaisePropertyChanged("PersonInsuranceDocumentStateString");
            }
        }

        private DateTime endDate = DateTime.MinValue;
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                Set("EndDate", ref endDate, value);
                RaisePropertyChanged("PersonInsuranceDocumentState");
                RaisePropertyChanged("PersonInsuranceDocumentStateString");
            }
        }

        private bool withoutEndDate;
        public bool WithoutEndDate
        {
            get { return withoutEndDate; }
            set
            {
                Set("WithoutEndDate", ref withoutEndDate, value);
                EndDate = DateTime.MaxValue;
                RaisePropertyChanged("WithEndDate");
            }
        }

        public bool WithEndDate
        {
            get { return !WithoutEndDate; }
        }

        public ItemState PersonInsuranceDocumentState
        {
            get
            {
                var datetimeNow = DateTime.Now;
                if (datetimeNow >= BeginDate && datetimeNow < EndDate)
                    return ItemState.Active;
                return ItemState.Inactive;
            }
        }

        public string PersonInsuranceDocumentStateString
        {
            get
            {
                switch (PersonInsuranceDocumentState)
                {
                    case ItemState.Active:
                        return "Действующий документ";
                    case ItemState.Inactive:
                        return "Недействующий документ";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
