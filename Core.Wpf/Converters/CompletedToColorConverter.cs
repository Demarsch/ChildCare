using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Core.Wpf.Converters
{
    public class CompletedToColorConverter : IValueConverter
    {
        public static readonly CompletedToColorConverter Instance = new CompletedToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool?)value == true ? Colors.Green : Colors.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
