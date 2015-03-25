using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Registry
{
    class InsuranceDocumentViewModel : ObservableObject
    {
        InsuranceDocument insuranceDocument;

        public InsuranceDocumentViewModel(InsuranceDocument insuranceDocument)
        {
            this.insuranceDocument = insuranceDocument;
        }

        private string insuranceCompany = string.Empty;
        public string InsuranceCompany
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
