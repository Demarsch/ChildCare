using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMessageModule
{
    public static class ModuleStrings
    {
        public const string MessageTypesLoading = "Загрузка типов сообщений...";
        public const string MessageTypesLoadingError = "Не удалось загрузить типы сообщений.";
        public const string MessageSelectorAllItem = "Все сообщения";
        public const string TabHeaderTitle = "Сообщения";
        public const string TabHeaderTipBegin = "У вас ";
        public const string TabHeaderTipEnd = " не прочитанных сообщений!";
        public const string TabHeaderTipEnd1 = " не прочитанное сообщение!";
        public const string TabHeaderTipEnd234 = " не прочитанных сообщения!";
        public const string MessagesLoading = "Загрузка сообщений...";
        public const string MessagesLoadingError = "Не удалось загрузить сообщения.";
        public const string MessageReadError = "Не удалось отметить сообщение как прочитанное.";

        public const string ShowReadSetting = "UserMessageInbox/ShowReadMessages";
        public const string ReadOpenedSetting = "UserMessageInbox/MarkOpenedAsRead";
    }
}
