using System;
using System.Windows.Markup;

namespace Core
{
    public class NullableBooleanExtension : MarkupExtension
    {
        public NullableBooleanExtension(bool value)
        {
            Value = value;
        }

        [ConstructorArgument("value")]
        public bool Value { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return (bool?)Value;
        }
    }
}
