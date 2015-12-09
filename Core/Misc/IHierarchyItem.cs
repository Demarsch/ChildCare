using System.Collections.Generic;

namespace Core.Misc
{
    public interface IHierarchyItem
    {
        IHierarchyItem Parent { get; }

        IEnumerable<IHierarchyItem> Children { get; }
    }
}
