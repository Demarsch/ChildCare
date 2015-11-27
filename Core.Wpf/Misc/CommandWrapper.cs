using System;
using System.Windows.Input;
using Prism.Mvvm;

namespace Core.Wpf.Misc
{
    public sealed class CommandWrapper : BindableBase, ICommand, IDisposable
    {
        public static CommandWrapper Empty = new CommandWrapper();

        private ICommand command;

        public ICommand Command
        {
            get { return command; }
            set
            {
                if (command != null)
                {
                    command.CanExecuteChanged -= CommandOnCanExecuteChanged;
                }
                if (value != null)
                {
                    value.CanExecuteChanged += CommandOnCanExecuteChanged;

                }
                if (SetProperty(ref command, value))
                {
                    OnPropertyChanged(() => HasCommand);
                }
            }
        }

        private void CommandOnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            OnCanExecuteChanged();
        }

        private string commandName;

        public string CommandName
        {
            get { return commandName; }
            set
            {
                if (SetProperty(ref commandName, value))
                {
                    OnPropertyChanged(() => HasCommand);
                }
            }
        }

        private object commandParameter;

        public object CommandParameter
        {
            get { return commandParameter; }
            set { SetProperty(ref commandParameter, value); }
        }

        public bool HasCommand
        {
            get { return Command != null && !string.IsNullOrWhiteSpace(CommandName); }
        }

        public bool CanExecute(object parameter)
        {
            return command.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            command.Execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if (command != null)
            {
                command.CanExecuteChanged -= CommandOnCanExecuteChanged;
            }
        }
    }
}
