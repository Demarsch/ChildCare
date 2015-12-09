using System;
using System.Collections.Generic;
using Core.Misc;

namespace Core.Data
{
    public partial class RecordType : IHierarchyItem
    {
        public static readonly Predicate<object> AssignableRecordTypeSelectionPredicate = x => ((RecordType)x).Assignable == true;
        
        IHierarchyItem IHierarchyItem.Parent { get { return RecordType1; } }

        IEnumerable<IHierarchyItem> IHierarchyItem.Children { get { return RecordTypes1; } }
    }
}
