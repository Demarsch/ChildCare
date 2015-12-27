using System;

namespace Core.Notification
{
    public class NotificationEventArgs<TItem> : EventArgs
    {
        public NotificationEventArgs(TItem oldItem, TItem newItem)
        {
            OldItem = oldItem;
            NewItem = newItem;
        }

        public TItem OldItem { get; private set; }

        public TItem NewItem { get; private set; }
    }
}