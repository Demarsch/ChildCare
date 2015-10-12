using System;
using System.Globalization;
using System.Windows.Data;

namespace Registry
{
    public class IsPrintedToColorConverter : IValueConverter
    {
        public static readonly IsPrintedToColorConverter Instance = new IsPrintedToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is bool)
            {
                var val = (bool)value;
                if (val)
                    return "LightGreen";
                else
                    return "LightPink";
            }
            return "White";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
