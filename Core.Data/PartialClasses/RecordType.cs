using System;
using System.Collections.Generic;
using System.Linq;
using Core.Misc;

namespace Core.Data
{
    public partial class RecordType : IHierarchyItem
    {
        public static readonly Predicate<object> AssignableRecordTypeSelectionPredicate = x => ((RecordType)x).Assignable == true;

        public static readonly Func<object, string, bool> AssignableRecordTypeFilterPredicate = AssignableRecordTypeFilter;

        private static readonly char[] Separators = { ' ' };

        private static bool AssignableRecordTypeFilter(object o, string s)
        {
            var recordType = (RecordType)o;
            var words = s.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            return words.All(x => recordType.Name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        IHierarchyItem IHierarchyItem.Parent { get { return RecordType1; } }

        IEnumerable<IHierarchyItem> IHierarchyItem.Children { get { return RecordTypes1; } }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Id, Name);
        }
    }
}
