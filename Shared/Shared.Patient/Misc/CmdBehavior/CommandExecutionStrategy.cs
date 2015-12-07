using System;

namespace Shared.Patient.Misc
{
    #region Namespace

    

    #endregion

    /// <summary>
    /// Provides implementation for execution strategy.
    /// </summary>
    public class CommandExecutionStrategy : IExecutionStrategy
    {
        /// <summary>
        /// Gets or sets the Behavior that we execute this strategy
        /// </summary>
        public CommandBehaviorBinding Behavior { get; set; }

        /// <summary>
        /// Executes the Command that is stored in the CommandProperty of the CommandExecution
        /// </summary>
        /// <param name="parameter">The parameter for the command</param>
        public void Execute(object parameter)
        {
            if (null == Behavior)
            {
                throw new InvalidOperationException("Behavior property cannot be null when executing a strategy");
            }

            if (Behavior.Command.CanExecute(Behavior.CommandParameter))
            {
                Behavior.Command.Execute(Behavior.CommandParameter);
            }
        }
    }
}
