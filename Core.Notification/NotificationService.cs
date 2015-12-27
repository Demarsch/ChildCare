using System;
using System.Linq.Expressions;
using System.ServiceModel;
using log4net;

namespace Core.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ILog log;

        public NotificationService(ILog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.log = log;
        }

        public INotificationServiceSubscription<TItem> Subscribe<TItem>(Expression<Predicate<TItem>> filter = null) where TItem : class, new()
        {
            try
            {
                var result = new NotificationServiceSubscription<TItem>(log, filter);
                log.InfoFormat("Successfully subscribe to {0} notifications{1}", typeof(TItem).Name, filter == null ? string.Empty : " with filter");
                return result;
            }
            catch (EndpointNotFoundException ex)
            {
                log.Error("Failed to create subscription. NotificationServiceEngine is offline", ex);
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Failed to create subscription. Unknown error", ex);
                return null;
            }
        }
    }
}