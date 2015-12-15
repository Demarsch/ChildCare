using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Core.Wpf.Behaviors
{
    public static class FocusAdvancement
    {
        public static FocusAdvancementDirection GetFocusAdvancementDirection(DependencyObject obj)
        {
            return (FocusAdvancementDirection)obj.GetValue(FocusAdvancementDirectionProperty);
        }

        public static void SetFocusAdvancementDirection(DependencyObject obj, FocusAdvancementDirection value)
        {
            obj.SetValue(FocusAdvancementDirectionProperty, value);
        }

        public static readonly DependencyProperty FocusAdvancementDirectionProperty = DependencyProperty.RegisterAttached("FocusAdvancementDirection",
                                                                                                                          typeof(FocusAdvancementDirection), 
                                                                                                                          typeof(FocusAdvancement), 
                                                                                                                          new UIPropertyMetadata(FocusAdvancementDirection.None, OnFocusAdvancementDirectionChanged));

        private static void OnFocusAdvancementDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element == null)
            {
                return;
            }
            var newValue = (FocusAdvancementDirection)e.NewValue;
            if (newValue == FocusAdvancementDirection.None)
            {
                element.PreviewKeyDown -= PreviewKeyDown;
            }
            else
            {
                element.PreviewKeyDown -= PreviewKeyDown;
                element.PreviewKeyDown += PreviewKeyDown;
            }
        }

        private static void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var element = (UIElement)sender;
            var focusAdvancementDirection = GetFocusAdvancementDirection(element);
            var goNext = e.Key == Key.Enter && (focusAdvancementDirection == FocusAdvancementDirection.ForwardOnly || focusAdvancementDirection == FocusAdvancementDirection.Both);
            if (goNext)
            {
                e.Handled = true;
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                return;
            }
            var goPrevious = e.Key == Key.Back && (focusAdvancementDirection == FocusAdvancementDirection.BackwardOnly || focusAdvancementDirection == FocusAdvancementDirection.Both);
            if (goPrevious)
            {
                var elementAllowsToGoBack = true;
                if (element is TextBox)
                {
                    elementAllowsToGoBack = string.IsNullOrEmpty((element as TextBox).Text);
                }
                else if (element is DatePicker)
                {
                    elementAllowsToGoBack = string.IsNullOrEmpty((element as DatePicker).Text);
                }
                if (elementAllowsToGoBack)
                {
                    e.Handled = true;
                    element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                }
            }
        }
    }

    public enum FocusAdvancementDirection
    {
        None,
        ForwardOnly,
        BackwardOnly,
        Both
    }
}