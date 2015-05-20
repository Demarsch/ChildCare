using System;
using System.Globalization;
using System.Windows.Data;

namespace Core
{
    public class DiagnosticsConverter : IValueConverter
    {
        public static readonly DiagnosticsConverter Instance = new DiagnosticsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
