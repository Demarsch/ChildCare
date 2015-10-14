using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class BusyMediator : BindableBase
    {
        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            private set { SetProperty(ref isBusy, value); }
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
            IsBusy = true;
        }

        public void Deactivate()
        {
            IsBusy = false;
        }
    }
}
