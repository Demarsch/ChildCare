using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLib
{
    public partial class Person
    {
        
        public string LastNameTo(DateTime date)
        {
            var lastPersonName = PersonNames.FirstOrDefault(x => date >= x.BeginDateTime && date < x.EndDateTime);
            return lastPersonName == null ? string.Empty : lastPersonName.LastName;
        }
        
        public string FirstNameTo(DateTime date)
        {
                var lastPersonName = PersonNames.LastOrDefault(x => date >= x.BeginDateTime && date < x.EndDateTime);
                return lastPersonName == null ? string.Empty : lastPersonName.FirstName;
        }
        
        public string MiddleNameTo(DateTime date)
        {
                var lastPersonName = PersonNames.LastOrDefault(x => date >= x.BeginDateTime && date < x.EndDateTime);
                return lastPersonName == null ? string.Empty : lastPersonName.MiddleName;
        }

        public IEnumerable<InsuranceDocument> GetActualInsuranceDocuments(DateTime date)
        {
            return this.InsuranceDocuments.Where(x => date >= x.BeginDate && date < x.EndDate);
        }

        public string TodayActualInsuranceDocumentStrings
        {
            get 
            {
                var resStr = string.Empty;
                foreach (var insuranceDocument in this.GetActualInsuranceDocuments(DateTime.Now))
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += insuranceDocument.InsuranceDocumentString;
                }
                return resStr;
            }
        }

        //ToDo: get photo from store
        public string PhotoUri
        {
            get
            {
                return string.Empty;
            }
        }
    }
}
