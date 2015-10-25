using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class GenderToImageConverter : IValueConverter
    {
        public static readonly GenderToImageConverter Instance = new GenderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            return new Uri(boolValue ? "pack://application:,,,/Core;Component/Resources/Images/Man48x48.png"
                                     : "pack://application:,,,/Core;Component/Resources/Images/Woman48x48.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}