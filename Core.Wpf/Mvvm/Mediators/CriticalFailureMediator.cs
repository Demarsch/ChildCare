using System.Windows.Input;
using Core.Wpf.Misc;
using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class CriticalFailureMediator : BindableBase, IMediator
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

        private CommandWrapper retryCommand;

        public CommandWrapper RetryCommand
        {
            get { return retryCommand; }
            set { SetProperty(ref retryCommand, value); }
        }

        public void Activate(object failureMessage)
        {
            Activate(failureMessage, CommandWrapper.Empty);
        }

        public void Activate(object failureMessage, CommandWrapper retryCommand)
        {
            Message = failureMessage;
            RetryCommand = retryCommand;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
