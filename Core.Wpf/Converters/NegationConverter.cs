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
                return -(int)value;
            }
            if (value is double)
            {
                return -(double)value;
            }
            if (value is float)
            {
                return -(float)value;
            }
            if (value is decimal)
            {
                return -(decimal)value;
            }
            if (value is long)
            {
                return -(long)value;
            }
            throw new NotSupportedException(string.Format("Value '{0}' of type '{1}' doesn't support negation operation", value, value == null ? "???" : value.GetType().ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
