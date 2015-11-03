using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class ProportionConverter : IValueConverter
    {
        public double MultiplyBy { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            if (value is double)
            {
                return (double)value * MultiplyBy;
            }
            if (value is int)
            {
                return (int)value * MultiplyBy;
            }
            if (value is decimal)
            {
                return (decimal)value * (decimal)MultiplyBy;
            }
            if (value is short)
            {
                return (short)value * MultiplyBy;
            }
            if (value is long)
            {
                return (long)value * MultiplyBy;
            }
            throw new ConverterException("Value must be of numeric type to be converted");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
