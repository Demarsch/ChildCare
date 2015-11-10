using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Core.Misc;
using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public sealed class ObservableCollectionChangeTracker<TItem> : BindableBase, IChangeTracker
    {
        private IEnumerable<TItem> originalItems;

        private readonly ObservableCollectionEx<TItem> collection;

        public ObservableCollectionChangeTracker(ObservableCollectionEx<TItem> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.collection = collection;
            collection.BeforeCollectionChanged += OnBeforeCollectionChanged;
            originalItems = null;
        }

        private void OnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (originalItems == null && IsEnabled)
            {
                originalItems = collection.ToArray();
            }
        }

        public bool PropertyHasChanges(string propertyName)
        {
            //This tracker doesn't track separate properties of items
            return false;
        }

        public void AcceptChanges()
        {
            originalItems = null;
            HasChanges = false;
        }

        public void RestoreChanges()
        {
            collection.Replace(originalItems);
            originalItems = null;
            HasChanges = false;
        }

        public void RegisterComparer(string propertyName, IEqualityComparer comparer)
        {
            //This tracker doesn't use comparers to track properties
        }

        public void Dispose()
        {
            collection.BeforeCollectionChanged -= OnBeforeCollectionChanged;
        }

        private bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (SetProperty(ref isEnabled, value) && !value)
                {
                    AcceptChanges();
                }
            }
        }

        private bool hasChanges;

        public bool HasChanges
        {
            get { return hasChanges; }
            private set { SetProperty(ref hasChanges, value); }
        }
    }
}
