using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class TalonStateToImageConverter : IValueConverter
    {
        public static readonly TalonStateToImageConverter Instance = new TalonStateToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool?)value;
            if (!boolValue.HasValue)
                return new Uri("pack://application:,,,/Core;Component/Resources/Images/Record48x48.png");
            else if (boolValue == false)
                return new Uri("pack://application:,,,/Core;Component/Resources/Images/RecordInProgress48x48.png");
            return new Uri("pack://application:,,,/Core;Component/Resources/Images/RecordCompleted48x48.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}