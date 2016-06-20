using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Core.Wpf.Mvvm
{
    public class ObservableCollectionEx<TItem> : ObservableCollection<TItem>
    {
        public ObservableCollectionEx()
        { }

        public ObservableCollectionEx(IEnumerable<TItem> collection)
            : base(collection)
        { }

        public ObservableCollectionEx(List<TItem> list)
            : base(list)
        { }

        public void AddRange(IEnumerable<TItem> items)
        {
            if (items == null)
            {
                return;
            }
            var newItems = items.ToArray();
            if (newItems.Length > 0)
            {
                OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
            }
            foreach (var item in newItems)
            {
                SubscribeOnItemNotifyPropertyChanged(item);
                Items.Add(item);
            }
            if (newItems.Length == 0)
            {
                return;
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void SubscribeOnItemNotifyPropertyChanged(TItem item)
        {
            var notifyPropertyChangedItem = item as INotifyPropertyChanged;
            if (notifyPropertyChangedItem != null)
                notifyPropertyChangedItem.PropertyChanged += notifyPropertyChangedItem_PropertyChanged;
        }

        private void UnsubscribeOnItemNotifyPropertyChanged(TItem item)
        {
            var notifyPropertyChangedItem = item as INotifyPropertyChanged;
            if (notifyPropertyChangedItem != null)
                notifyPropertyChangedItem.PropertyChanged -= notifyPropertyChangedItem_PropertyChanged;
        }

        private void UnsubscribeOnItemNotifyPropertyChanged()
        {
            foreach (var item in base.Items)
            {
                UnsubscribeOnItemNotifyPropertyChanged(item);
            }
        }

        void notifyPropertyChangedItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnItemPropertyChanged(e);
        }

        public event PropertyChangedEventHandler ItemPropertyChanged;

        public event NotifyCollectionChangedEventHandler BeforeCollectionChanged;

        protected virtual void OnItemPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = ItemPropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnBeforeCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = BeforeCollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void ClearItems()
        {
            if (Items.Count > 0)
            {
                OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Items as IList));
                UnsubscribeOnItemNotifyPropertyChanged();
            }
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            if (index < Items.Count)
            {
                OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Items[index], index));
                UnsubscribeOnItemNotifyPropertyChanged(base[index]);
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TItem item)
        {
            if (index < Items.Count)
            {
                OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, Items[index], index));
                UnsubscribeOnItemNotifyPropertyChanged(base[index]);
            }
            base.SetItem(index, item);
            SubscribeOnItemNotifyPropertyChanged(base[index]);
        }

        protected override void InsertItem(int index, TItem item)
        {
            OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            base.InsertItem(index, item);
            SubscribeOnItemNotifyPropertyChanged(item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Items[oldIndex], newIndex, oldIndex));
            base.MoveItem(oldIndex, newIndex);
        }

        public void Replace(IEnumerable<TItem> newItems)
        {
            var newItemsArray = newItems == null ? new TItem[0] : newItems.ToArray();
            var wasChanged = Items.Count > 0 || newItemsArray.Length > 0;
            if (wasChanged)
            {
                OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList)newItemsArray, (IList)Items));
            }
            UnsubscribeOnItemNotifyPropertyChanged();
            Items.Clear();
            foreach (var item in newItemsArray)
            {
                SubscribeOnItemNotifyPropertyChanged(item);
                Items.Add(item);
                wasChanged = true;
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
            var removedItems = new List<TItem>();
            var removedIndices = new List<int>();
            for (var index = Items.Count - 1; index >= 0; index--)
            {
                if (predicate(Items[index]))
                {
                    removedItems.Add(Items[index]);
                    removedIndices.Add(index);
                    UnsubscribeOnItemNotifyPropertyChanged(Items[index]);
                }
            }
            if (removedIndices.Count == 0)
            {
                return false;
            }
            OnBeforeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
            foreach (var index in removedIndices)
            {
                Items.RemoveAt(index);
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }
    }
}
