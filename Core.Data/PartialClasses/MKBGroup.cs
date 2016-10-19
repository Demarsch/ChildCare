using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public partial class MKBGroup
    {
        public static readonly Predicate<object> MKBGroupSelectorPredicate = MKBGroupSelector;

        private static bool MKBGroupSelector(object obj)
        {
            var group = (MKBGroup)obj;
            if (group == null)
            {
                return false;
            }
            return true;
        }

        public bool ContainsFindString(string findString)
        {
            if (string.IsNullOrEmpty(findString) || string.IsNullOrEmpty(this.Name))
                return false;
            return this.Name.IndexOf(findString, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}
