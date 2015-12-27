using System;
using System.Linq.Expressions;

namespace Core.Notification
{
    public interface INotificationService
    {
        INotificationServiceSubscription<TItem> Subscribe<TItem>(Expression<Predicate<TItem>> filter = null) where TItem : class, new();
    }
}