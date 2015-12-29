using System;

namespace Core.Notification
{
    public static class NotificationServiceSubscriptionExtensions
    {
        public static void NotifyCreate<TItem>(this INotificationServiceSubscription<TItem> subscription, TItem newItem) where TItem : class, new()
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }
            subscription.Notify(null, newItem);
        }

        public static void NotifyDelete<TItem>(this INotificationServiceSubscription<TItem> subscription, TItem originalItem) where TItem : class, new()
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }
            subscription.Notify(originalItem, null);
        }
    }
}
