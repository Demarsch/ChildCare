using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Core.Wpf.Misc;

namespace Core.Wpf.Behaviors
{
    public class StringProcessorBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty StringProcessorProperty = DependencyProperty.Register("StringProcessor", typeof(IStringProcessor), typeof(StringProcessorBehavior), new PropertyMetadata(OnStringProcessorChanged));

        private static void OnStringProcessorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (StringProcessorBehavior)d;
            if (behavior.AssociatedObject != null)
            {
                behavior.textIsChangedByStringProcessor = true;
                behavior.AssociatedObject.Text = behavior.StringProcessorResolved.ProcessString(behavior.AssociatedObject.Text);
                behavior.textIsChangedByStringProcessor = false;
            }
        }

        public IStringProcessor StringProcessor
        {
            get { return (IStringProcessor)GetValue(StringProcessorProperty); }
            set { SetValue(StringProcessorProperty, value); }
        }

        private IStringProcessor StringProcessorResolved
        {
            get { return StringProcessor ?? NoOpStringProcessor.Instance; }
        }

        private bool textIsChangedByStringProcessor;

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

        private void AssociatedObjectOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (textIsChangedByStringProcessor)
            {
                return;
            }
            textIsChangedByStringProcessor = true;
            var caretIndex = AssociatedObject.CaretIndex;
            var isAtTheEnd = caretIndex == AssociatedObject.Text.Length;
            AssociatedObject.Text = StringProcessorResolved.ProcessString(AssociatedObject.Text);
            AssociatedObject.CaretIndex = isAtTheEnd ? AssociatedObject.Text.Length : caretIndex;
            textIsChangedByStringProcessor = false;
        }

        private void AssociatedObjectOnPreviewTextInput(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            ;
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
