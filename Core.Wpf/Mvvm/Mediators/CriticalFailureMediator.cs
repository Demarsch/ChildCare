using System.Windows.Input;
using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class CriticalFailureMediator : BindableBase
    {
        public const string RetryCommandDefaultName = "Повторить";

        private bool hasFailure;

        public bool HasFailure
        {
            get { return hasFailure; }
            private set { SetProperty(ref hasFailure, value); }
        }

        private object message;

        public object Message
        {
            get { return message; }
            private set { SetProperty(ref message, value); }
        }

        private ICommand retryCommand;

        public ICommand RetryCommand
        {
            get { return retryCommand; }
            private set { SetProperty(ref retryCommand, value); }
        }

        private string retryCommandName;

        public string RetryCommandName
        {
            get { return retryCommandName; }
            private set { SetProperty(ref retryCommandName, value); }
        }

        public void Activate(object failureMessage, ICommand retryCommand, string retryCommandName = RetryCommandDefaultName)
        {
            Message = failureMessage;
            RetryCommand = retryCommand;
            HasFailure = true;
            RetryCommandName = retryCommandName;
        }

        public void Deactivate()
        {
            HasFailure = false;
            RetryCommand = null;
            Message = null;
            RetryCommandName = string.Empty;
        }
    }
}
