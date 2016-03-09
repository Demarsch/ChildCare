using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class AllMembersToImageConverter : IValueConverter
    {
        public static readonly AllMembersToImageConverter Instance = new AllMembersToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            return new Uri(boolValue ? "pack://application:,,,/Core;Component/Resources/Images/AllMembers.png"
                                     : "pack://application:,,,/Core;Component/Resources/Images/AnyMember.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}