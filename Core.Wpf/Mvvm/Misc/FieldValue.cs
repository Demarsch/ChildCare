using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class FieldValue : BindableBase
    {
        public int Value { get; set; }

        public string Field { get; set; }
    }
}
