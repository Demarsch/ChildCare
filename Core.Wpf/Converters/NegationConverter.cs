using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class NegationConverter : IValueConverter
    {
        public static readonly NegationConverter Instance = new NegationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                return ((parameter != null && parameter is int) ? (int)parameter : 0) - (int)value;
            }
            if (value is double)
            {
                return ((parameter != null && parameter is double) ? (double)parameter : 0) - (double)value;
            }
            if (value is float)
            {
                return ((parameter != null && parameter is float) ? (float)parameter : 0) - (float)value;
            }
            if (value is decimal)
            {
                return ((parameter != null && parameter is decimal) ? (decimal)parameter : 0) - (decimal)value;
            }
            if (value is long)
            {
                return ((parameter != null && parameter is long) ? (long)parameter : 0) - (long)value;
            }
            throw new NotSupportedException(string.Format("Value '{0}' of type '{1}' doesn't support negation operation", value, value == null ? "???" : value.GetType().ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
