using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Misc;

namespace Core.Data
{
    [DebuggerDisplay("{Id} - {Name}")]
    public partial class RecordType : IHierarchyItem<RecordType>
    {
        public static readonly Predicate<object> AssignableRecordTypeSelectorPredicate = AssignableRecordTypeSelector;

        private static bool AssignableRecordTypeSelector(object obj)
        {
            var recordType = (RecordType)obj;
            if (recordType == null)
            {
                return false;
            }
            return recordType.Assignable == true;
        }

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

        protected bool Equals(RecordType other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((RecordType)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
