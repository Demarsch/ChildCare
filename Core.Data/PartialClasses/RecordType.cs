using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Misc;

namespace Core.Data
{
    [DebuggerDisplay("{Id} - {Name}")]
    public partial class RecordType : IHierarchyItem<RecordType>, IHierarchyItem
    {
        public static readonly Predicate<object> AssignableRecordTypeSelectionPredicate = x => ((RecordType)x).Assignable == true;

        public static readonly Func<object, string, bool> AssignableRecordTypeFilterPredicate = AssignableRecordTypeFilter;

        private static readonly char[] Separators = { ' ' };

        private static bool AssignableRecordTypeFilter(object item, string filter)
        {
            var recordType = (RecordType)item;
            var words = filter.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            return words.All(x => recordType.Name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        IHierarchyItem IHierarchyItem.Parent { get { return Parent; } }

        IEnumerable<IHierarchyItem> IHierarchyItem.Children { get { return Children; } }

        public IEnumerable<RecordType> Children { get { return RecordTypes1; } }

        public RecordType Parent { get { return RecordType1; } }
    }
}
