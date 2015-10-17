using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Core.Wpf.Converters
{
    [ContentProperty("Items")]
    public class Converter : IValueConverter
    {
        public Converter()
        {
            Items = new List<ConverterItem>();
        }

        public List<ConverterItem> Items { get; private set; }

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

        public bool KeepSourceValueOnDefault { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in Items)
            {
                if (Equals(item.From, value))
                {
                    return item.To;
                }
            }
            if (useDefaultValue)
            {
                return DefaultValue;
            }
            if (KeepSourceValueOnDefault)
            {
                return value;
            }
            throw new ConverterException(string.Format("Value {0} can't be converted and default value is not allowed", value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in Items)
            {
                if (Equals(item.To, value))
                {
                    return item.From;
                }
            }
            throw new ConverterException(string.Format("Value {0} can't be converted back", value));
        }
    }

    public class ConverterItem
    {
        public object From { get; set; }

        public object To { get; set; }
    }

    public class ConverterException : Exception
    {
        public ConverterException()
        {
        }

        public ConverterException(string message) : base(message)
        {
        }
    }
}