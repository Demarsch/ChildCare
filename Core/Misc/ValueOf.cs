namespace Core.Misc
{
    public class ValueOf<TItem>
    {
        public static readonly ValueOf<TItem> Empty = new ValueOf<TItem>(); 

        private ValueOf()
        {
            HasValue = false;
        }

        public ValueOf(TItem value)
        {
            Value = value;
            HasValue = true;
        }

        public TItem Value { get; private set; }

        public bool HasValue { get; private set; }
    }
}
