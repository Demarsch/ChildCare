using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Notification;
using System.Threading;
using Core.Misc;

namespace UserMessagerModule.Services
{
    public class UserMessageService : IUserMessageService
    {
        IDbContextProvider contextProvider;
        IUserService userService;
        ICacheService cacheService;
        INotificationService notificationService;

        public UserMessageService(IDbContextProvider contextProvider, IUserService userService, ICacheService cacheService, INotificationService notificationService)
        {
            if ((this.contextProvider = contextProvider) == null)
                throw new ArgumentNullException("contextProvider");
            if ((this.userService = userService) == null)
                throw new ArgumentNullException("userService");
            if ((this.cacheService = cacheService) == null)
                throw new ArgumentNullException("cacheService");
            if ((this.notificationService = notificationService) == null)
                throw new ArgumentNullException("notificationService");
        }

        public async void SendMessageAsync(int recieverUserId, int messageTypeId, string messageText, string messageTag, int outDays)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var message = new UserMessage()
                {
                    MessageTag = messageTag,
                    MessageText = messageText,
                    OutDateTime = DateTime.Now.AddDays(outDays > 0 ? outDays : 30),
                    ReadDateTime = null,
                    RecieverUserId = recieverUserId,
                    SendDateTime = DateTime.Now,
                    SenderUserId = userService.GetCurrentUserId(),
                    UserMessageTypeId = messageTypeId
                };
                var tonotify = (UserMessage)message.Clone();
                context.Set<UserMessage>().Add(message);
                await context.SaveChangesAsync();

                using (var sb = notificationService.Subscribe<UserMessage>())
                    if (sb != null) sb.NotifyCreate(tonotify);
            }
        }

        public IDisposableQueryable<UserMessage> GetMessages(int senderUserId, int recieverUserId, DateTime toDate)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<UserMessage>(context.Set<UserMessage>().Where(x => (senderUserId == 0 || x.SenderUserId == senderUserId) && (recieverUserId == 0 || x.RecieverUserId == recieverUserId) && (!x.OutDateTime.HasValue || x.OutDateTime > toDate)) ,context);
        }

        public UserMessageType GetMessageTypeById(int id)
        {
            return cacheService.GetItemById<UserMessageType>(id);
        }

        public async void SetMessageReadAsync(int messageId, DateTime? readDateTime)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var message = context.Set<UserMessage>().FirstOrDefault(x => x.Id == messageId);
                var tonotifyold = (UserMessage)message.Clone();
                message.ReadDateTime = readDateTime;
                var tonotifynew = (UserMessage)message.Clone();
                await context.SaveChangesAsync();

                using (var sb = notificationService.Subscribe<UserMessage>())
                    if (sb != null) sb.Notify(tonotifyold, tonotifynew);
            }
        }

    }
}
