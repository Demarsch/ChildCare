using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Prism.Mvvm;

namespace Core.Wpf.ViewModels
{
    public class ActionRequiringNotificationViewModel : BindableBase
    {
        public ActionRequiringNotificationViewModel()
        {
            Actions = new ObservableCollectionEx<CommandWrapper>();
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        public ObservableCollectionEx<CommandWrapper> Actions { get; private set; }
    }
}
