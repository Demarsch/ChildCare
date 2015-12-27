using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;

namespace NotificationServiceEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class NotificationServiceEngine : INotificationServiceEngine
    {
        private readonly Dictionary<string, HashSet<INotificationServiceEngineCallback>> subscriptions;

        private readonly ILog log;

        public NotificationServiceEngine()
        {
            subscriptions = new Dictionary<string, HashSet<INotificationServiceEngineCallback>>();
            log = LogManager.GetLogger(typeof(NotificationServiceEngine));
        }

        public void Subscribe(string subscriptionType)
        {
            var currentSubscriber = OperationContext.Current.GetCallbackChannel<INotificationServiceEngineCallback>();
            log.InfoFormat("Client subscribes to {0} events", subscriptionType);
            lock (subscriptions)
            {
                HashSet<INotificationServiceEngineCallback> currentTypeSubscriptions;
                if (!subscriptions.TryGetValue(subscriptionType, out currentTypeSubscriptions))
                {
                    currentTypeSubscriptions = new HashSet<INotificationServiceEngineCallback>();
                    subscriptions.Add(subscriptionType, currentTypeSubscriptions);
                }
                currentTypeSubscriptions.Add(currentSubscriber);
            }
        }

        public void Notify(string subscriptionType, byte[] oldItem, byte[] newItem)
        {
            var currentSubscriber = OperationContext.Current.GetCallbackChannel<INotificationServiceEngineCallback>();
            var operation = oldItem == null
                ? "creating"
                : newItem == null
                    ? "deleting"
                    : "updating";
            lock (subscriptions)
            {
                HashSet<INotificationServiceEngineCallback> currentTypeSubscriptions;
                if (!subscriptions.TryGetValue(subscriptionType, out currentTypeSubscriptions))
                {
                    return;
                }
                log.InfoFormat("Client notifies {0} other client(s) about {1} of {2}",
                    currentTypeSubscriptions.Count - 1,
                    operation,
                    subscriptionType);
                var offlineSubscribers = new List<INotificationServiceEngineCallback>();
                foreach (var subscriber in currentTypeSubscriptions.Where(x => !x.Equals(currentSubscriber)))
                {
                    if (((ICommunicationObject)subscriber).State != CommunicationState.Opened)
                    {
                        log.InfoFormat("Client seems to be offline. Removing him from the list");
                        offlineSubscribers.Add(subscriber);
                    }
                    else
                    {
                        try
                        {
                            subscriber.OnNotified(oldItem, newItem);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Client was online but failed to recieve notification. Removing him from the list", ex);
                            offlineSubscribers.Add(subscriber);
                        }
                    }
                }
                if (offlineSubscribers.Count > 0)
                {
                    currentTypeSubscriptions.ExceptWith(offlineSubscribers);
                    log.InfoFormat("Removed {0} offline subscribers", offlineSubscribers.Count);
                }
            }
        }

        public void Unsubscribe(string subscriptionType)
        {
            var currentSubscriber = OperationContext.Current.GetCallbackChannel<INotificationServiceEngineCallback>();
            lock (subscriptions)
            {
                HashSet<INotificationServiceEngineCallback> currentTypeSubscriptions;
                if (!subscriptions.TryGetValue(subscriptionType, out currentTypeSubscriptions))
                {
                    return;
                }
                log.InfoFormat("Client unsubscribes from {0} events", subscriptionType);
                currentTypeSubscriptions.Remove(currentSubscriber);
            }
        }
    }
}
