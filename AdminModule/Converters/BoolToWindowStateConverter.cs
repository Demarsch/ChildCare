using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdminModule.Converters
{
    public class BoolToWindowStateConverter : IValueConverter
    {
        public static readonly BoolToWindowStateConverter Instance = new BoolToWindowStateConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool?)value;
            return boolValue == true ? Xceed.Wpf.Toolkit.WindowState.Open : Xceed.Wpf.Toolkit.WindowState.Closed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Xceed.Wpf.Toolkit.WindowState)value;
            return visibilityValue == Xceed.Wpf.Toolkit.WindowState.Open;
        }
    }
}
