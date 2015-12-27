using System.ServiceModel;

namespace NotificationServiceEngine
{
    [ServiceContract(CallbackContract = typeof(INotificationServiceEngineCallback), SessionMode = SessionMode.Required)]
    public interface INotificationServiceEngine
    {
        [OperationContract]
        void Subscribe(string subscriptionType);

        [OperationContract(IsOneWay = true)]
        void Notify(string subscriptionType, byte[] oldItem, byte[] newItem);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(string subscriptionType);
    }
}
