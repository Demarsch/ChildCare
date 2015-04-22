using System;
using System.Globalization;
using System.Windows.Data;

namespace Registry
{
    public class TimeToHeightConverter : IValueConverter
    {
        public static readonly double DefaultUnitsPerMinute = 3.0;

        public static readonly TimeToHeightConverter DefaultInstance = new TimeToHeightConverter();

        public TimeToHeightConverter()
        {
            UnitsPerMinute = DefaultUnitsPerMinute;
        }

        public double UnitsPerMinute { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var realValue = value is DateTime ? ((DateTime)value).TimeOfDay : (TimeSpan)value;
            return realValue.TotalMinutes * UnitsPerMinute;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
