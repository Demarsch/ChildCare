using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public partial class InsuranceCompany
    {
        public override bool Equals(object obj)
        {
            var insuranceCompany2 = obj as InsuranceCompany;
            if (insuranceCompany2 == null)
                return false;
            if (this.Id < 1 || insuranceCompany2.Id < 1)
                return false;
            return this.Id == insuranceCompany2.Id;
        }
    }
}
