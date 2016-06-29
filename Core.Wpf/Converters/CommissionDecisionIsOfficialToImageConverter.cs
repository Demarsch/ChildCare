using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class CommissionDecisionIsOfficialToImageConverter : IValueConverter
    {
        public static readonly CommissionDecisionIsOfficialToImageConverter Instance = new CommissionDecisionIsOfficialToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            return new Uri(boolValue ? "pack://application:,,,/Core;Component/Resources/Images/CommissionUseDecision.png"
                                     : "pack://application:,,,/Core;Component/Resources/Images/CommissionDoNotUseDecision.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}