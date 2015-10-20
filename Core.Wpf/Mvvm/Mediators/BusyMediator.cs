using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class BusyMediator : BindableBase, IMediator
    {
        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            private set { SetProperty(ref isActive, value); }
        }

        private object message;

        public object Message
        {
            get { return message; }
            private set { SetProperty(ref message, value); }
        }

        public void Activate(object busyMessage)
        {
            Message = busyMessage;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
