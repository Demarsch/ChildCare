using System;
using System.Windows;
using System.Windows.Input;
using Core.Wpf.Misc;
using Prism.Commands;
using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class FailureMediator : BindableBase, IMediator
    {
        public FailureMediator()
        {
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);
            deactivateCommand = new DelegateCommand(Deactivate, () => CanBeDeactivated);
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

        private readonly DelegateCommand deactivateCommand;

        public ICommand DeactivateCommand { get { return deactivateCommand; } }

        private bool canBeDeactivated;

        public bool CanBeDeactivated
        {
            get { return canBeDeactivated; }
            private set
            {
                if (SetProperty(ref canBeDeactivated, value))
                {
                    deactivateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand CopyToClipboardCommand { get; private set; }

        private void CopyToClipboard()
        {
            if (HasException)
            {
                Clipboard.SetText(Exception.ToString());
            }
        }

        public void Activate(object failureMessage, CommandWrapper retryCommand = null, Exception exception = null, bool canBeDeactivated = false)
        {
            Message = failureMessage;
            RetryCommand = retryCommand ?? CommandWrapper.Empty;
            Exception = exception;
            CanBeDeactivated = canBeDeactivated;
            IsActive = true;
        }

        public void Deactivate()
        {
            if (canBeDeactivated)
            {
                IsActive = false;
            }
        }
    }
}
