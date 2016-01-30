using System;
using System.Globalization;
using System.Windows.Data;
using Core.Misc;

namespace Core.Wpf.Converters
{
    public class DateToUserFriendlyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var @date = (DateTime)value;
            @date = @date.Date;
            if (@date == DateTime.Today.AddDays(-2.0))
            {
                return "Позавчера";
            }
            if (@date == DateTime.Today.AddDays(-1.0))
            {
                return "Вчера";
            }
            if (@date == DateTime.Today)
            {
                return "Сегодня";
            }
            if (@date == DateTime.Today.AddDays(1.0))
            {
                return "Завтра";
            }
            if (@date == DateTime.Today.AddDays(2.0))
            {
                return "Послезавтра";
            }
            var format = (parameter ?? DateTimeFormats.ShortDateFormat).ToString();
            return @date.ToString(format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
