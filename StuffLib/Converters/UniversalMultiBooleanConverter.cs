using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace StuffLib
{
    public class MultiBooleanConverter : IMultiValueConverter
    {
        public bool UseLogicalOr { get; set; }

        public object TrueValue { get; set; }

        public object FalseValue { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var realValues = values.Cast<bool?>().Select(x => x.HasValue && x.Value);
            if (UseLogicalOr)
                return realValues.Any(x => x) ? TrueValue : FalseValue;
            return realValues.All(x => x) ? TrueValue : FalseValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
