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

        private string message;

        public string Message
        {
            get { return message; }
            private set { SetProperty(ref message, value); }
        }

        public void Activate(string busyMessage)
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
