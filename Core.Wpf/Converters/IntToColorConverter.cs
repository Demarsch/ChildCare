using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Core.Wpf.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        public static readonly IntToColorConverter Instance = new IntToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? intValue = (int?)value;
            if (intValue == null || intValue == -1)
                return Colors.White;
            if (intValue == 0)
                return Colors.LightPink;
            else if (intValue == 1)
                return Colors.LemonChiffon;
            else if (intValue == 2)
                return Colors.LightGreen;
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
