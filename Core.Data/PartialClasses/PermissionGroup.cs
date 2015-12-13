using System.Collections.Generic;
using System.Diagnostics;
using Core.Misc;

namespace Core.Data
{
    [DebuggerDisplay("{Id} - {Name}")]
    public partial class PermissionGroup : IHierarchyItem<PermissionGroup>
    {
        IHierarchyItem IHierarchyItem.Parent { get { return Parent; } }

        public IEnumerable<PermissionGroup> Children { get { return PermissionGroups1; } }

        public PermissionGroup Parent { get { return PermissionGroup1; } }

        IEnumerable<IHierarchyItem> IHierarchyItem.Children { get { return Children; } }
    }
}
