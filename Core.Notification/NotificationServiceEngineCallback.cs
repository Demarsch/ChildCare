using System;
using NotificationServiceEngine;

namespace Core.Notification
{
    internal class NotificationServiceEngineCallback<TItem> : INotificationServiceEngineCallback where TItem : class, new()
    {
        public void OnNotified(byte[] oldItem, byte[] newItem)
        {
            Notified(this, new NotificationEventArgs<TItem>(oldItem.Deserialize<TItem>(), newItem.Deserialize<TItem>()));
        }

        public event EventHandler<NotificationEventArgs<TItem>> Notified = delegate { };
    }
}
