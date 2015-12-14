namespace Core.Wpf.Misc
{
    public class NoOpStringProcessor : IStringProcessor
    {
        public static readonly NoOpStringProcessor Instance = new NoOpStringProcessor();

        public string ProcessString(string input)
        {
            return input;
        }
    }
}