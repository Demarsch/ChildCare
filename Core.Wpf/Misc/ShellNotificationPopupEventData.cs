using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Wpf.Misc
{
    public class ShellNotificationPopupEventData
    {
        public ShellNotificationPopupEventData(string popupCaption, string popupMessageText, Action<object> popupClickAction = null, object popupClickActionData = null)
        {
            PopupCaption = popupCaption;
            PopupMessageText = popupMessageText;
            PopupClickAction = popupClickAction;
            PopupClickActionData = popupClickActionData;
        }

        public Action<object> PopupClickAction { get; set; }
        public object PopupClickActionData { get; set; }
        public string PopupCaption { get; set; }
        public string PopupMessageText { get; set; }
    }
}
