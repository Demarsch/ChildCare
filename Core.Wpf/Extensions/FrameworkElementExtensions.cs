using System.Windows;

namespace Core.Wpf.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static T FindAncestor<T>(this FrameworkElement element) where T : FrameworkElement
        {
            var parent = element.Parent as FrameworkElement;
            while (parent != null)
            {
                var result = parent as T;
                if (result != null)
                {
                    return result;
                }
                parent = parent.Parent as FrameworkElement;
            }
            return null;
        }
    }
}
