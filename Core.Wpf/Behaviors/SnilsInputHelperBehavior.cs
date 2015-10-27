using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Core.Wpf.Behaviors
{
    public class SnilsInputHelperBehavior : Behavior<TextBox>
    {
        private const int FullSnilsLength = 14;

        private const int FirstDashIndex = 3;

        private const int SecondDashIndex = 7;

        private const int SpaceIndex = 11;

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
            var properSnils = new StringBuilder();
            var snilsIndex = 0;
            var textIndex = 0;
            var text = AssociatedObject.Text.Where(char.IsDigit).ToArray();
            while (textIndex < text.Length && snilsIndex < FullSnilsLength)
            {
                if (snilsIndex == SpaceIndex)
                {
                    properSnils.Append(' ');
                    textIndex--;
                }
                else if (snilsIndex == FirstDashIndex || snilsIndex == SecondDashIndex)
                {
                    properSnils.Append('-');
                    textIndex--;
                }
                else
                {
                    properSnils.Append(text[textIndex]);
                }
                textIndex++;
                snilsIndex++;
            }
            ignoreTextChanged = true;
            var caretIndex = AssociatedObject.CaretIndex;
            var isAtTheEnd = caretIndex == AssociatedObject.Text.Length;
            AssociatedObject.Text = properSnils.ToString();
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
