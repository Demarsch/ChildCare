using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Core
{
    public class UniversalConverter : List<ConverterItem>, IValueConverter
    {
        private bool useDefaultValue;

        private object defaultValue;

        public object DefaultValue
        {
            get { return defaultValue; }
            set
            {
                defaultValue = value;
                useDefaultValue = true;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in this)
                if (Equals(item.From, value))
                    return item.To;
            if (useDefaultValue)
                return DefaultValue;
            throw new ConversionException(string.Format("Value {0} can't be converted and default value is not allowed", value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in this)
                if (Equals(item.To, value))
                    return item.From;
            throw new ConversionException(string.Format("Value {0} can't be converted back", value));
        }
    }

    public class ConverterItem
    {
        public object From { get; set; }

        public object To { get; set; }
    }

    public class ConversionException : Exception
    {
        public ConversionException() { }

        public ConversionException(string message) : base(message) { }
    }
}
