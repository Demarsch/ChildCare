using Core.Data;
using Core.Data.Misc;
using Core.Notification;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UserMessageModule.Services
{
    public interface IUserMessageService
    {
        void SendUserMessage(int recieverUserId, int messageTypeId, string messageText, string messageTag, bool highPriority, int outDays, bool resendAllways);

        IDisposableQueryable<UserMessage> GetMessages(int senderUserId, int recieverUserId, DateTime toDate);

        UserMessageType GetMessageTypeById(int id);

        void SetMessageReadDateTime(int messageId, DateTime? readDateTime);
    }
}
