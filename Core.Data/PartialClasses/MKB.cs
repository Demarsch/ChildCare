using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public partial class MKB
    {
        public override string ToString()
        {
            return this.Code + " " + this.Name;
        }

        public bool ContainsFindString(string findString)
        {
            if (string.IsNullOrEmpty(findString) || (string.IsNullOrEmpty(this.Name) && string.IsNullOrEmpty(this.Code)))
                return false;

            return this.Name.IndexOf(findString, StringComparison.InvariantCultureIgnoreCase) > -1 || this.Code.IndexOf(findString, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}
