using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Notification
{
    public interface INotificationService
    {
        INotificationServiceSubscription<TItem> Subscribe<TItem>(Expression<Predicate<TItem>> filter = null) where TItem : class, new();

        string ServiceBaseAddress { get; }

        Task CheckServiceExistsAsync();
    }
}