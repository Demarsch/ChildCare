using System;

namespace Core.Notification
{
    public class NotificationEventArgs<TItem> : EventArgs where TItem : class
    {
        public NotificationEventArgs(TItem oldItem, TItem newItem)
        {
            if (oldItem == null && newItem == null)
            {
                throw new ArgumentNullException("newItem", "Both oldItem and newItem can't be null");
            }
            OldItem = oldItem;
            NewItem = newItem;
        }

        public TItem OldItem { get; private set; }

        public TItem NewItem { get; private set; }

        public bool IsCreate { get { return OldItem == null; } }

        public bool IsUpdate { get { return OldItem != null && NewItem != null; } }

        public bool IsDelete { get { return NewItem == null; } }
    }
}