using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class BoolToCollapsedConverter : IValueConverter
    {
        public static readonly BoolToCollapsedConverter Instance = new BoolToCollapsedConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool?)value;
            return boolValue == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility)value;
            return visibilityValue == Visibility.Visible;
        }
    }
}
