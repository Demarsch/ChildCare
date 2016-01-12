using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Core.Wpf.Converters
{
    public class MultiBoolToVisibilityConverter : IMultiValueConverter
    {
        public MultiBoolToVisibilityConverter()
        {
            Condition = MultiBoolCondition.AllTrue;
            PositiveResult = Visibility.Visible;
            NegativeResult = Visibility.Collapsed;
        }

        public MultiBoolCondition Condition { get; set; }

        public Visibility PositiveResult { get; set; }

        public Visibility NegativeResult { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var bools = values.Select(x => x is bool? ? (bool?)x : false).Select(x => x.GetValueOrDefault());
            switch (Condition)
            {
                case MultiBoolCondition.AllTrue:
                    return bools.All(x => x) ? PositiveResult : NegativeResult;
                case MultiBoolCondition.AllFalse:
                    return bools.All(x => !x) ? PositiveResult : NegativeResult;
                case MultiBoolCondition.Mixed:
                    var hasTrue = false;
                    var hasFalse = false;
                    foreach (var @bool in bools)
                    {
                        hasTrue = hasTrue || @bool;
                        hasFalse = hasFalse || !@bool;
                    }
                    return hasTrue && hasFalse ? PositiveResult : NegativeResult;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum MultiBoolCondition
    {
        AllTrue,
        AllFalse,
        Mixed,
    }
}
