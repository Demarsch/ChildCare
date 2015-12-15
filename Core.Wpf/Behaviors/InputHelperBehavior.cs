using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Core.Wpf.Misc;

namespace Core.Wpf.Behaviors
{
    public class InputHelperBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty InputHelperProperty = DependencyProperty.Register("InputHelper", typeof(IInputHelper), typeof(InputHelperBehavior), new PropertyMetadata(OnStringProcessorChanged));

        private static void OnStringProcessorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (InputHelperBehavior)d;
            if (behavior.AssociatedObject != null)
            {
                behavior.textIsChangedByInputHelper = true;
                var result = behavior.InputHelperResolved.ProcessInput(behavior.AssociatedObject.Text);
                if (!result.InputCanBeContinued)
                {
                    behavior.GoToNextElementIfPossible();
                }
                behavior.AssociatedObject.Text = result.Output;
                behavior.textIsChangedByInputHelper = false;
            }
        }

        public IInputHelper InputHelper
        {
            get { return (IInputHelper)GetValue(InputHelperProperty); }
            set { SetValue(InputHelperProperty, value); }
        }

        public static readonly DependencyProperty GoToNextElementAfterInputCompletedProperty = DependencyProperty.Register("GoToNextElementAfterInputCompleted", typeof (bool), typeof (InputHelperBehavior), new PropertyMetadata(true));

        public bool GoToNextElementAfterInputCompleted
        {
            get { return (bool)GetValue(GoToNextElementAfterInputCompletedProperty); }
            set { SetValue(GoToNextElementAfterInputCompletedProperty, value); }
        }

        private IInputHelper InputHelperResolved
        {
            get { return InputHelper ?? NoOpInputHelper.Instance; }
        }

        private bool textIsChangedByInputHelper;

        protected override void OnAttached()
        {
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

        private void AssociatedObjectOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (textIsChangedByInputHelper)
            {
                return;
            }
            textIsChangedByInputHelper = true;
            var caretIndex = AssociatedObject.CaretIndex;
            var isAtTheEnd = caretIndex == AssociatedObject.Text.Length;
            var result = InputHelperResolved.ProcessInput(AssociatedObject.Text);
            AssociatedObject.Text = result.Output;
            if (!result.InputCanBeContinued)
            {
                GoToNextElementIfPossible();
            }
            AssociatedObject.CaretIndex = isAtTheEnd ? AssociatedObject.Text.Length : caretIndex;
            textIsChangedByInputHelper = false;
        }

        private void GoToNextElementIfPossible()
        {
            if (AssociatedObject.IsKeyboardFocused && GoToNextElementAfterInputCompleted)
            {
                AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            CommandManager.RemovePreviewExecutedHandler(AssociatedObject, OnPreviewTextCommandExecute);
            base.OnDetaching();
        }
    }
}
