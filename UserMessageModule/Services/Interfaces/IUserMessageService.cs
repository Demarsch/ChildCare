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
        void SendMessageAsync(int recieverUserId, int messageTypeId, string messageText, string messageTag, int outDays);

        IDisposableQueryable<UserMessage> GetMessages(int senderUserId, int recieverUserId, DateTime toDate);

        UserMessageType GetMessageTypeById(int id);

        void SetMessageReadAsync(int messageId, DateTime? readDateTime);
    }
}
