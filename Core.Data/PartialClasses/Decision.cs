using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public partial class Decision
    {
        public static readonly Func<object, string, bool> DecisionFilterPredicate = DecisionFilter;

        private static readonly char[] Separators = { ' ' };

        private static bool DecisionFilter(object item, string filter)
        {
            var decision = (Decision)item;
            var words = filter.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            return words.All(x => decision.Name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1);
        }
    }
}
