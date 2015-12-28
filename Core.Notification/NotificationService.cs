using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using log4net;

namespace Core.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ILog log;

        private readonly IDbContextProvider contextProvider;

        public NotificationService(ILog log, IDbContextProvider contextProvider)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.log = log;
            this.contextProvider = contextProvider;
        }

        public INotificationServiceSubscription<TItem> Subscribe<TItem>(Expression<Predicate<TItem>> filter = null) where TItem : class, new()
        {
            try
            {
                var result = new NotificationServiceSubscription<TItem>(log, this, filter);
                log.InfoFormat("Successfully subscribe to {0} notifications{1}", typeof (TItem).Name, filter == null ? string.Empty : " with filter");
                return result;
            }
            catch (DataNotFoundException ex)
            {
                log.Error("Failed to create subscription. " + ex.Message, ex);
                return null;
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

        private string serviceBaseAddress;

        public string ServiceBaseAddress
        {
            get
            {
                if (serviceBaseAddress == null)
                {
                    using (var context = contextProvider.CreateLightweightContext())
                    {
                        serviceBaseAddress = context.Set<DBSetting>().Where(x => x.Name == DBSetting.NotificationServiceAddress).Select(x => x.Value).FirstOrDefault();
                        if (serviceBaseAddress == null)
                        {
                            throw new DataNotFoundException("There is no address of NotificationService stored in database");
                        }
                    }
                }
                return serviceBaseAddress;
            }
        }

        public async Task CheckServiceExistsAsync()
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                serviceBaseAddress = await context.Set<DBSetting>().Where(x => x.Name == DBSetting.NotificationServiceAddress).Select(x => x.Value).FirstOrDefaultAsync();
                if (serviceBaseAddress == null)
                {
                    throw new DataNotFoundException("There is no address of NotificationService stored in database");
                }
            }
        }
    }
}