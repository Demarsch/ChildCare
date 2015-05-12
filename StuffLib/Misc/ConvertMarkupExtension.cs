using System;
using System.Windows.Markup;

namespace Core
{
    public class ConvertExtension : MarkupExtension
    {
        public ConvertExtension() { }

        public ConvertExtension(string value, Type targetType)
        {
            Value = value;
            TargetType = targetType;
        }

        [ConstructorArgument("targetType")]
        public Type TargetType { get; set; }

        [ConstructorArgument("value")]
        public string Value { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Convert.ChangeType(Value, TargetType);
        }
    }
}
