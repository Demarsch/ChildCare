﻿using DataLib;
using System;
using GalaSoft.MvvmLight;

namespace Registry
{
    class InsuranceDocumentViewModel : ObservableObject
    {
        private readonly InsuranceDocument insuranceDocument;

        public InsuranceDocumentViewModel(InsuranceDocument insuranceDocument)
        {
            if (insuranceDocument == null)
                throw new ArgumentNullException("insuranceDocument");
            this.insuranceDocument = insuranceDocument;
            FillData();
        }

        private void FillData()
        {
            InsuranceCompanyId = insuranceDocument.InsuranceCompanyId;
            InsuranceDocumentTypeId = insuranceDocument.InsuranceDocumentTypeId;
            Series = insuranceDocument.Series;
            Number = insuranceDocument.Number;
            BeginDate = insuranceDocument.BeginDate;
            EndDate = insuranceDocument.EndDate;
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
            set { Set("BeginDate", ref beginDate, value); }
        }

        private DateTime endDate = DateTime.MinValue;
        public DateTime EndDate
        {
            get { return endDate; }
            set { Set("EndDate", ref endDate, value); }
        }
    }
}