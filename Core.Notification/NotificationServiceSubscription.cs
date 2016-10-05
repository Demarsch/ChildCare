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

        public NotificationServiceSubscription(ILog log, INotificationService notificationService, Expression<Predicate<TItem>> filterPredicate = null)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (notificationService == null)
            {
                throw new ArgumentNullException("notificationService");
            }
            this.log = log;
            if (filterPredicate != null)
            {
                filter = filterPredicate.Compile();
            }
            callback = new NotificationServiceEngineCallback<TItem>();
            callback.Notified += CallbackOnNotified;
            client = new NotificationServiceEngineClient(new InstanceContext(callback));
            client.Endpoint.Address = new EndpointAddress(string.Join("/", new[] { notificationService.ServiceBaseAddress, client.Endpoint.Address == null ? string.Empty : client.Endpoint.Address.Uri.ToString() }));
            client.Open();
            client.Subscribe(typeof(TItem));
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
                    client.Unsubscribe(typeof(TItem));
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

        public void Notify(TItem originalItem, TItem newItem)
        {
            try
            {
                client.Notify(typeof(TItem), originalItem.Serialize(), newItem.Serialize());
            }
            catch (Exception ex)
            {
                log.Error("Failed to notify NotificationServiceEngine about changes", ex);
            }
        }
    }
}