using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class AllMembersToTextConverter : IValueConverter
    {
        public static readonly AllMembersToTextConverter Instance = new AllMembersToTextConverter();

        public bool ShortFormat { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            return boolValue ? "Решения всех участинков" : "Хотя бы одно решение";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
