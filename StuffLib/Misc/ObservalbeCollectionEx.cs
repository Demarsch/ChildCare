using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Core
{
    public class ObservalbeCollectionEx<TItem> : ObservableCollection<TItem>
    {
        public ObservalbeCollectionEx()
        {}

        public ObservalbeCollectionEx(IEnumerable<TItem> collection)
            : base(collection)
        {}

        public ObservalbeCollectionEx(List<TItem> list)
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

        public void Replace(IEnumerable<TItem> items)
        {
            var wasChanged = Items.Count > 0;
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
            for (var index = Items.Count - 1; index >= 0; index--)
            {
                if (predicate(Items[index]))
                {
                    Items.RemoveAt(index);
                    wasRemoved = true;
                }
            }
            if (!wasRemoved)
            {
                return false;
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }
    }
}
