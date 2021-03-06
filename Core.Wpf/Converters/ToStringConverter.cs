﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class ToStringConverter : IValueConverter
    {
        public static readonly ToStringConverter Instance = new ToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            if ((parameter as string) == null)
                return value.ToString();
            return string.Format(culture, parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
