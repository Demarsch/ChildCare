using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class ElementAtConverter : IValueConverter
    {
        public static readonly ElementAtConverter Instance = new ElementAtConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((object[])parameter)[Math.Max(Math.Min((value is bool) ? ((bool)value ? 1 : 0) : (int)value, ((object[])parameter).Length), 0)];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}