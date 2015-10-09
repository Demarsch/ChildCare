using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLib
{
    public partial class PersonStaff
    {        
        public string PersonName
        {
            get
            {
                return this.Person.ShortName;
            }
        }
    }
}
