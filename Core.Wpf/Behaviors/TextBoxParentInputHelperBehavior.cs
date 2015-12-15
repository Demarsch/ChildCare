using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Core.Wpf.Misc;
using Xceed.Wpf.AvalonDock.Controls;

namespace Core.Wpf.Behaviors
{
    public class TextBoxParentInputHelperBehavior : Behavior<FrameworkElement>
    {
        private TextBox childTextBox;

        private TextBoxInputHelperBehavior childBehavior = new TextBoxInputHelperBehavior();

        public static readonly DependencyProperty InputHelperProperty = DependencyProperty.Register("InputHelper", typeof(IInputHelper), typeof(TextBoxParentInputHelperBehavior), new PropertyMetadata(OnInputHelperChanged));

        private static void OnInputHelperChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (TextBoxParentInputHelperBehavior)d;
            behavior.childBehavior.InputHelper = e.NewValue as IInputHelper;
        }

        public IInputHelper InputHelper
        {
            get { return (IInputHelper)GetValue(InputHelperProperty); }
            set { SetValue(InputHelperProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            FindAndSetChildTextBox();
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
           FindAndSetChildTextBox();
        }

        private void FindAndSetChildTextBox()
        {
            var newTextBox = AssociatedObject.FindVisualChildren<TextBox>().FirstOrDefault();
            if (childTextBox != null)
            {
                childBehavior.Detach();
            }
            childTextBox = newTextBox;
            if (childTextBox != null)
            {
                childBehavior.Attach(childTextBox);
                childBehavior.InputHelper = InputHelper;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
            childBehavior.Detach();
            childTextBox = null;
            childBehavior = null;
            base.OnDetaching();
        }
    }
}