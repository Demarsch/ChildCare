using System.Collections.Generic;

namespace Core.Misc
{
    public interface IHierarchyItem
    {
        IHierarchyItem Parent { get; }

        IEnumerable<IHierarchyItem> Children { get; }
    }

    public interface IHierarchyItem<out TItem> : IHierarchyItem where TItem : IHierarchyItem
    {
        new TItem Parent { get; }

        new IEnumerable<TItem> Children { get; } 
    }
}
