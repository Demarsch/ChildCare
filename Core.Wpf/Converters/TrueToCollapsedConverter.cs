using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class TrueToCollapsedConverter : IValueConverter
    {
        public static readonly TrueToCollapsedConverter Instance = new TrueToCollapsedConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool?)value;
            return boolValue == true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility)value;
            return visibilityValue == Visibility.Collapsed;
        }
    }
}
