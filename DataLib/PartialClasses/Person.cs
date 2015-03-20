using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public partial class Person
    {
        public string CurrentLastName
        {
            get
            {
                if (this.PersonNames.Any())
                    return this.PersonNames.Last().LastName;
                return string.Empty;
            }
        }

        public string CurrentFirstName
        {
            get
            {
                if (this.PersonNames.Any())
                    return this.PersonNames.Last().FirstName;
                return string.Empty;
            }
        }

        public string CurrentMiddleName
        {
            get
            {
                if (this.PersonNames.Any())
                    return this.PersonNames.Last().FirstName;
                return string.Empty;
            }
        }
    }
}
