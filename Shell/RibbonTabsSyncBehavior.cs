using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.Extensions;
using Fluent;
using Prism.Regions;
using Prism.Regions.Behaviors;

namespace Shell
{
    public class RibbonTabsSyncBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
        public static readonly string BehaviorKey = "RibbonTabsSyncBehavior";

        private bool updatingActiveViewsInRibbonSelectedTabChanged;

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
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var startIndex = e.NewStartingIndex;
                foreach (var newItem in e.NewItems.Cast<RibbonTabItem>())
                {
                    ribbon.Tabs.Insert(startIndex++, newItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems.Cast<RibbonTabItem>())
                {
                    ribbon.Tabs.Remove(oldItem);
                }
            }
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


                foreach (var item in e.RemovedItems.Cast<RibbonTabItem>())
                {
                    // check if the view is in both Views and ActiveViews collections (there may be out of sync)
                    if (Region.Views.Contains(item) && Region.ActiveViews.Contains(item))
                    {
                        Region.Deactivate(item);
                    }
                }
                foreach (var item in e.AddedItems.Cast<RibbonTabItem>())
                {
                    if (Region.Views.Contains(item) && !Region.ActiveViews.Contains(item))
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
