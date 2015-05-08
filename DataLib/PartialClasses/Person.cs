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
