using System;
using System.Linq.Expressions;
using System.ServiceModel;
using Core.Notification.NotificationServiceEngine;
using log4net;

namespace Core.Notification
{
    internal class NotificationServiceSubscription<TItem> : INotificationServiceSubscription<TItem> where TItem : class, new()
    {
        private readonly ILog log;

        private readonly Predicate<TItem> filter;

        private NotificationServiceEngineClient client;

        private readonly NotificationServiceEngineCallback<TItem> callback;

        public NotificationServiceSubscription(ILog log, Expression<Predicate<TItem>> filterPredicate = null)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.log = log;
            if (filterPredicate != null)
            {
                filter = filterPredicate.Compile();
            }
            callback = new NotificationServiceEngineCallback<TItem>();
            callback.Notified += CallbackOnNotified; 
            client = new NotificationServiceEngineClient(new InstanceContext(callback));
            client.Open();
            client.Subscribe(typeof(TItem).FullName);
        }

        private void CallbackOnNotified(object sender, NotificationEventArgs<TItem> e)
        {
            if (filter == null
                || (e.OldItem != null && filter(e.OldItem))
                || (e.NewItem != null && filter(e.NewItem)))
            {
                Notified(this, e);
            }
        }

        public void Dispose()
        {
            callback.Notified -= CallbackOnNotified;
            if (client != null)
            {
                try
                {
                    client.Unsubscribe(typeof(TItem).FullName);
                    client.Close();
                }
                catch
                {
                    client.Abort();
                }
                client = null;
            }
            log.InfoFormat("Unsubscribed from {0} events", typeof(TItem).FullName);
        }

        public event EventHandler<NotificationEventArgs<TItem>> Notified = delegate { };

        public void Notify(TItem oldItem, TItem newItem)
        {
            try
            {
                client.Notify(typeof(TItem).FullName, oldItem.Serialize(), newItem.Serialize());
            }
            catch (Exception ex)
            {
                log.Error("Failed to notify NotificationServiceEngine about changes", ex);
            }
        }
    }
}