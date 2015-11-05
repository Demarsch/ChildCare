using System;
using System.Windows;
using System.Windows.Input;
using Core.Wpf.Misc;
using Prism.Commands;
using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class CriticalFailureMediator : BindableBase, IMediator
    {
        public CriticalFailureMediator()
        {
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);
        }

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

        private Exception exception;

        public Exception Exception
        {
            get { return exception; }
            set
            {
                if (SetProperty(ref exception, value))
                {
                    OnPropertyChanged(() => HasException);
                }
            }
        }

        public bool HasException { get { return Exception != null; } }

        public ICommand CopyToClipboardCommand { get; private set; }

        private void CopyToClipboard()
        {
            if (HasException)
            {
                Clipboard.SetText(Exception.ToString());
            }
        }

        public void Activate(object failureMessage)
        {
            Activate(failureMessage, CommandWrapper.Empty, null);
        }

        public void Activate(object failureMessage, CommandWrapper retryCommand, Exception exception = null)
        {
            Message = failureMessage;
            RetryCommand = retryCommand;
            Exception = exception;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
