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
        private TItem[] originalItems;

        private readonly ObservableCollectionEx<TItem> collection;

        public ObservableCollectionChangeTracker(ObservableCollectionEx<TItem> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.collection = collection;
            collection.BeforeCollectionChanged += OnBeforeCollectionChanged;
            collection.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            if (originalItems.Length == 0 && collection.Count == 0)
            {
                AcceptChanges();
            }
            else
            {
                HasChanges = true;
            }
        }

        private void OnBeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (!IsEnabled)
            {
                return;
            }
            if (originalItems == null)
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
            if (originalItems != null)
            {
                collection.Replace(originalItems);
                originalItems = null;
                HasChanges = false;
            }
        }

        public void RegisterComparer(string propertyName, IEqualityComparer comparer)
        {
            //This tracker doesn't use comparers to track properties
        }

        public void Dispose()
        {
            collection.BeforeCollectionChanged -= OnBeforeCollectionChanged;
            collection.CollectionChanged -= OnCollectionChanged;
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
