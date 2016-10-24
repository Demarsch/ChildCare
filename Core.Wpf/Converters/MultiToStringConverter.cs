using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class MultiToStringConverter : IMultiValueConverter
    {
        public static readonly MultiToStringConverter Instance = new MultiToStringConverter();

        public object Convert(object []value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((parameter as string) == null)
                return value.ToString();
            return string.Format(culture, parameter as string, value);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
