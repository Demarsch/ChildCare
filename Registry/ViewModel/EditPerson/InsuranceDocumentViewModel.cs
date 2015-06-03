using DataLib;
using System;
using GalaSoft.MvvmLight;
using Core;
using log4net;
using System.ComponentModel;
using System.Collections.Generic;

namespace Registry
{
    public class InsuranceDocumentViewModel : ObservableObject, IDataErrorInfo
    {

        private readonly IPersonService service;

        private InsuranceDocument insuranceDocument;

        public InsuranceDocumentViewModel(InsuranceDocument insuranceDocument, IPersonService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.insuranceDocument = insuranceDocument;
            FillData();
        }

        public InsuranceDocument SetData()
        {
            if (insuranceDocument == null)
                insuranceDocument = new InsuranceDocument();
            insuranceDocument.InsuranceCompanyId = InsuranceCompanyId;
            insuranceDocument.InsuranceDocumentTypeId = InsuranceDocumentTypeId;
            if (InsuranceCompany == null)
                insuranceDocument.InsuranceCompany = null;
            else
                insuranceDocument.InsuranceCompanyId = InsuranceCompany.Id;
            insuranceDocument.Series = Series;
            insuranceDocument.Number = Number;
            insuranceDocument.BeginDate = BeginDate;
            insuranceDocument.EndDate = EndDate;

            return insuranceDocument;
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
                if (withoutEndDate)
                    EndDate = DateTime.MaxValue;
                else
                    EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
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

        #region Implementation IDataErrorInfo

        private bool saveWasRequested { get; set; }

        public readonly HashSet<string> invalidProperties = new HashSet<string>();

        public bool Invalidate()
        {
            saveWasRequested = true;
            RaisePropertyChanged(string.Empty);
            return invalidProperties.Count < 1;
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "InsuranceCompany")
                {
                    result = InsuranceCompany == null ? "Укажите страховую компанию" : string.Empty;
                }
                if (columnName == "Number")
                {
                    result = Number == string.Empty ? "Укажите номер полиса" : string.Empty;
                }
                if (columnName == "BeginDate" || columnName == "EndDate")
                {
                    result = BeginDate > EndDate ? "Дата начала не может быть больше даты окончания" : string.Empty;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
