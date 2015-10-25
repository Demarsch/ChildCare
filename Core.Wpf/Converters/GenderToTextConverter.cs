using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class GenderToTextConverter : IValueConverter
    {
        public static readonly GenderToTextConverter Instance = new GenderToTextConverter();

        public bool ShortFormat { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            if (ShortFormat)
            {
                return boolValue ? "М" : "Ж";
            }
            return boolValue ? "Мужской" : "Женский";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
