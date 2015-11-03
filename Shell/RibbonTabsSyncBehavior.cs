using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Core.Extensions;
using Fluent;
using Prism.Regions;
using Prism.Regions.Behaviors;
using System.Linq;

namespace Shell
{
    public class RibbonTabsSyncBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
        public static readonly string BehaviorKey = "RibbonTabsSyncBehavior";

        private bool updatingActiveViewsInRibbonSelectedTabChanged;

        private List<string> viewSortHints; 

        private Ribbon ribbon;

        public DependencyObject HostControl
        {
            get { return ribbon; }
            set { ribbon = value as Ribbon; }
        }

        protected override void OnAttach()
        {
            SynchronizeItems();
            ribbon.SelectedTabChanged += OnRibbonSelectedTabChanged;
            Region.ActiveViews.CollectionChanged += OnActiveViewsCollectionChanged;
            Region.Views.CollectionChanged += OnViewCollectionChanged;
        }

        private void OnViewCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (viewSortHints == null)
            {
                viewSortHints = ribbon.Tabs.Select(GetViewSortHint).ToList();
            }
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems.Cast<RibbonTabItem>())
                {
                    var viewSortHint = GetViewSortHint(newItem);
                    var newItemPosition = GetNewItemPosition(viewSortHint);
                    ribbon.Tabs.Insert(newItemPosition, newItem);
                    viewSortHints.Insert(newItemPosition, viewSortHint);
                    if (newItem.Group != null && !ribbon.ContextualGroups.Contains(newItem.Group))
                    {
                        ribbon.ContextualGroups.Add(newItem.Group);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems.Cast<RibbonTabItem>())
                {
                    ribbon.Tabs.Remove(oldItem);
                    if (oldItem.Group != null && !ribbon.Tabs.Any(x => ReferenceEquals(x.Group, oldItem.Group)))
                    {
                        ribbon.ContextualGroups.Remove(oldItem.Group);
                    }
                }
            }
        }

        private int GetNewItemPosition(string viewSortHint)
        {
            if (viewSortHints.Count == 0)
            {
                return 0;
            }
            if (string.IsNullOrEmpty(viewSortHint))
            {
                return viewSortHints.Count;
            }
            if (string.IsNullOrEmpty(viewSortHints[0]))
            {
                return 0;
            }
            var index = 0;
            while (index < viewSortHints.Count)
            {
                if (string.IsNullOrEmpty(viewSortHints[index]) || string.CompareOrdinal(viewSortHint, viewSortHints[index]) < 0)
                {
                    break;
                }
                index++;
            }
            return index;
        }

        private string GetViewSortHint(RibbonTabItem ribbonTabItem)
        {
            var attribute = ribbonTabItem.GetType().GetCustomAttribute<ViewSortHintAttribute>();
            return attribute == null ? string.Empty : attribute.Hint;
        }

        private void SynchronizeItems()
        {
            var existingItems = new List<object>();
            foreach (var childItem in ribbon.Tabs)
            {
                existingItems.Add(childItem);
            }
            foreach (var view in Region.Views.Cast<RibbonTabItem>())
            {
                ribbon.Tabs.Add(view);
            }
            foreach (var existingItem in existingItems)
            {
                Region.Add(existingItem);
            }
        }


        private void OnActiveViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (updatingActiveViewsInRibbonSelectedTabChanged)
            {
                // If we are updating the ActiveViews collection in the OnRibbonSelectedTabChanged, that 
                // means the user has set the SelectedItem or SelectedItems himself and we don't need to do that here now
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (ribbon.SelectedTabItem != null
                    && !ReferenceEquals(ribbon.SelectedTabItem, e.NewItems[0])
                    && Region.ActiveViews.Contains(ribbon.SelectedTabItem))
                {
                    Region.Deactivate(ribbon.SelectedTabItem);
                }
                ribbon.SelectedTabItem = e.NewItems[0] as RibbonTabItem;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove &&
                     e.OldItems.Contains(ribbon.SelectedTabItem))
            {
                ribbon.SelectedTabItem = null;
            }
        }

        private void OnRibbonSelectedTabChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Record the fact that we are now updating active views in the OnRibbonSelectedTabChanged method. 
                // This is needed to prevent the OnActiveViewsCollectionChanged() method from firing. 
                updatingActiveViewsInRibbonSelectedTabChanged = true;


                foreach (var item in e.RemovedItems)
                {
                    // check if the view is in both Views and ActiveViews collections (there may be out of sync)
                    if (item is RibbonTabItem && Region.Views.Contains(item) && Region.ActiveViews.Contains(item))
                    {
                        Region.Deactivate(item);
                    }
                }
                foreach (var item in e.AddedItems)
                {
                    if (item is RibbonTabItem && Region.Views.Contains(item) && !Region.ActiveViews.Contains(item))
                    {
                        Region.Activate(item);
                    }
                }

            }
            finally
            {
                updatingActiveViewsInRibbonSelectedTabChanged = false;
            }
        }
    }
}
