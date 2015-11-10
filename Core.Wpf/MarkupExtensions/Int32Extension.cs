using System;
using System.Windows.Markup;

namespace Core.Wpf.MarkupExtensions
{
    public class Int32Extension : MarkupExtension
    {
        [ConstructorArgument("value")]
        public string Value { get; set; }

        public Int32Extension() { }

        public Int32Extension(string value)
        {
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return int.Parse(Value);
        }
    }
}
