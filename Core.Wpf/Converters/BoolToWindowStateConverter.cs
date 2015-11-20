using System;
using System.Globalization;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;

namespace Core.Wpf.Converters
{
    public class BoolToWindowStateConverter : IValueConverter
    {
        public static readonly BoolToWindowStateConverter Instance = new BoolToWindowStateConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool?)value;
            return boolValue == true ? WindowState.Open : WindowState.Closed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (WindowState)value;
            return visibilityValue == WindowState.Open;
        }
    }
}
