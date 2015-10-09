using GalaSoft.MvvmLight;

namespace Core
{
    public class FieldValue : ObservableObject
    {
        public int Value { get; set; }

        public string Field { get; set; }
    }
}
