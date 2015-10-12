using System.Windows;
using System.Windows.Controls;

namespace MainLib
{
    public static class ScrollViewerHelper
    {
        public static double GetHorizontalOffset(DependencyObject d)
        {
            return (double)d.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject d, double value)
        {
            d.SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerHelper), new PropertyMetadata(0.0, HorizontalOffsetChanged));

        private static void HorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer == null)
                return;
            scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
        }
    }
}
