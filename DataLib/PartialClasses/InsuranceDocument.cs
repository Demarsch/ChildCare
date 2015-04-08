using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public partial class InsuranceDocument
    {
        public string InsuranceDocumentString
        {
            get
            {
                return String.Format("тип док-та: {0}\r\nстрах. орг.: {1}\r\n серия {2} номер {3}\r\n период действия {4}-{5}",
                    this.InsuranceDocumentType.Name, this.InsuranceCompany.NameSMOK, this.Series, this.Number, this.BeginDate.ToShortDateString(),
                    this.EndDate.ToShortDateString());
            }
        }
    }
}
