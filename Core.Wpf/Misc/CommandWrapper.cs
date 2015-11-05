using System.Windows.Input;
using Prism.Mvvm;

namespace Core.Wpf.Misc
{
    public class CommandWrapper : BindableBase
    {
        public static readonly string DefaultCommandName = "Повторить";

        public static CommandWrapper Empty = new CommandWrapper();

        private ICommand command;

        public ICommand Command
        {
            get { return command; }
            set
            {
                if (SetProperty(ref command, value))
                {
                    OnPropertyChanged(() => HasCommand);
                }
            }
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
    }
}
