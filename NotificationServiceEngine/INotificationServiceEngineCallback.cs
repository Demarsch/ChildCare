using System.ServiceModel;

namespace NotificationServiceEngine
{
    public interface INotificationServiceEngineCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnNotified(byte[] oldItem, byte[] newItem);
    }
}
