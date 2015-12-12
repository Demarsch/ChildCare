using System;
using System.Collections.Generic;
using Core.Misc;

namespace Core.Extensions
{
    public static class HierarchyItemExtensions
    {
        public static bool IsParentOf<TItem>(this TItem item, TItem child) where TItem : class, IHierarchyItem
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            var parent = child.Parent;
            while (parent != null)
            {
                if (parent.Equals(item))
                {
                    return true;
                }
                parent = parent.Parent;
            }
            return false;
        }

        public static bool IsChildOf<TItem>(this TItem item, TItem parent) where TItem : class, IHierarchyItem
        {
            return parent.IsParentOf(item);
        }

        public static IEnumerable<TItem> GetAllParents<TItem>(this TItem item, bool takeItself = false) where TItem : class, IHierarchyItem<TItem>
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (takeItself)
            {
                yield return item;
            }
            var parent = item.Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        public static IEnumerable<TItem> GetAllChildren<TItem>(this TItem item, bool takeItself = false) where TItem : class, IHierarchyItem<TItem>
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            var result = new List<TItem>();
            if (takeItself)
            {
                result.Add(item);
            }
            result.AddRange(item.Children);
            foreach (var child in item.Children)
            {
                result.AddRange(child.GetAllChildren());
            }
            return result;
        }
    }
}
