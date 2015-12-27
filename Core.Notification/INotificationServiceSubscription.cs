using System;

namespace Core.Notification
{
    public interface INotificationServiceSubscription<TItem> : IDisposable where TItem : class, new()
    {
        event EventHandler<NotificationEventArgs<TItem>> Notified; 
        
        void Notify(TItem oldItem, TItem newItem);
    }
}