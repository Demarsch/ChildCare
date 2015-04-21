using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Core
{
    public class ToSharedSizeGroupConverter : IValueConverter
    {
        public static readonly ToSharedSizeGroupConverter DefaultInstance = new ToSharedSizeGroupConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new StringBuilder(value.ToString());
            for (var index = result.Length - 1; index >= 0; index--)
            {
                if (!char.IsLetterOrDigit(result[index]) && result[index] != '_')
                    result[index] = '_';
            }
            if (result.Length == 0 || char.IsDigit(result[0]))
                result.Insert(0, '_');
            return result.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
