using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Core.Wpf.Behaviors
{
    public class MouseWheelHorizontalScroll
    {
        public static readonly DependencyProperty PreferHorizontalScrollProperty = DependencyProperty.RegisterAttached("PreferHorizontalScroll", typeof(bool?), typeof(MouseWheelHorizontalScroll), new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)d;
            var newValue = (bool?)e.NewValue;
            if (newValue == false)
            {
                scrollViewer.PreviewMouseWheel -= OnScrollPreviewMouseWheel;
            }
            else
            {
                scrollViewer.PreviewMouseWheel += OnScrollPreviewMouseWheel;
            }
        }

        public static void SetPreferHorizontalScroll(DependencyObject element, bool? value)
        {
            element.SetValue(PreferHorizontalScrollProperty, value);
        }

        public static bool? GetPreferHorizontalScroll(DependencyObject element)
        {
            return (bool?)element.GetValue(PreferHorizontalScrollProperty);
        }

        private static void OnScrollPreviewMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
        {
            var scrollViewer = (ScrollViewer)sender;
            var preferHorizontal = (bool?)scrollViewer.GetValue(PreferHorizontalScrollProperty);
            if (scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible && preferHorizontal == null)
            {
                return;
            }
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - mouseWheelEventArgs.Delta);
            mouseWheelEventArgs.Handled = true;
        }
    }
}
