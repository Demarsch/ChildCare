using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public partial class Complication
    {
        public override string ToString()
        {
            return this.Name;
        }

        public bool ContainsFindString(string findString)
        {
            if (string.IsNullOrEmpty(findString))
                return false;

            return this.Name.IndexOf(findString, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}
