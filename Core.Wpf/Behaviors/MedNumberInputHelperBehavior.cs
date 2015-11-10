using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Core.Wpf.Behaviors
{
    public class MedNumberInputHelperBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PreviewTextInput += AssociatedObjectOnPreviewTextInput;
            AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            CommandManager.AddPreviewExecutedHandler(AssociatedObject, OnPreviewTextCommandExecute);
            base.OnAttached();
        }

        private void OnPreviewTextCommandExecute(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (executedRoutedEventArgs.Command == ApplicationCommands.Paste)
            {
                executedRoutedEventArgs.Handled = true;
            }
        }

        private bool ignoreTextChanged;

        private void AssociatedObjectOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (ignoreTextChanged)
            {
                return;
            }
            ignoreTextChanged = true;
            var caretIndex = AssociatedObject.CaretIndex;
            var isAtTheEnd = caretIndex == AssociatedObject.Text.Length;
            AssociatedObject.Text = AssociatedObject.Text.Trim();
            AssociatedObject.CaretIndex = isAtTheEnd ? AssociatedObject.Text.Length : caretIndex;
            ignoreTextChanged = false;
        }

        private void AssociatedObjectOnPreviewTextInput(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            if (textCompositionEventArgs.Text.Length > 0 && char.IsDigit(textCompositionEventArgs.Text[0]))
            {
                return;
            }
            textCompositionEventArgs.Handled = true;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= AssociatedObjectOnPreviewTextInput;
            AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            CommandManager.RemovePreviewExecutedHandler(AssociatedObject, OnPreviewTextCommandExecute);
            base.OnDetaching();
        }
    }
}
