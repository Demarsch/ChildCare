using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Core.Wpf.Mvvm
{
    public class ObservableCollectionEx<TItem> : ObservableCollection<TItem>
    {
        public ObservableCollectionEx()
        {}

        public ObservableCollectionEx(IEnumerable<TItem> collection)
            : base(collection)
        {}

        public ObservableCollectionEx(List<TItem> list)
            : base(list)
        {}

        public void AddRange(IEnumerable<TItem> items)
        {
            var wasAdded = false;
            if (items == null)
            {
                return;
            }
            foreach (var item in items)
            {
                Items.Add(item);
                wasAdded = true;
            }
            if (!wasAdded)
            {
                return;
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event EventHandler<IEnumerable<TItem>>  BeforeItemsRemoved;

        protected virtual void OnBeforeItemsRemoved(IEnumerable<TItem> e)
        {
            var handler = BeforeItemsRemoved;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void ClearItems()
        {
            OnBeforeItemsRemoved(Items);
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            OnBeforeItemsRemoved(new[] { Items[index]});
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TItem item)
        {
            OnBeforeItemsRemoved(new[] { Items[index]});
            base.SetItem(index, item);
        }

        public void Replace(IEnumerable<TItem> items)
        {
            var wasChanged = Items.Count > 0;
            if (wasChanged)
            {
                OnBeforeItemsRemoved(Items);
            }
            Items.Clear();
            if (items != null)
            {
                foreach (var item in items)
                {
                    Items.Add(item);
                    wasChanged = true;
                }
            }
            if (!wasChanged)
            {
                return;
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool RemoveWhere(Func<TItem, bool> predicate)
        {
            if (predicate == null)
            {
                return false;
            }
            var wasRemoved = false;
            var removedItems = new List<TItem>();
            for (var index = Items.Count - 1; index >= 0; index--)
            {
                if (predicate(Items[index]))
                {
                    removedItems.Add(Items[index]);
                    Items.RemoveAt(index);
                    wasRemoved = true;
                }
            }
            if (!wasRemoved)
            {
                return false;
            }
            OnBeforeItemsRemoved(removedItems);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }
    }
}
