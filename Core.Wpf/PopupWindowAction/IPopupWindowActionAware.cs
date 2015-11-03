using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Interactivity.InteractionRequest;

namespace Core.Wpf.PopupWindowActionAware
{
    public interface IPopupWindowActionAware
    {
        Window HostWindow { get; set; }

        INotification HostNotification { get; set; }
    }
}
