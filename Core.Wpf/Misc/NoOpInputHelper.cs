namespace Core.Wpf.Misc
{
    public class NoOpInputHelper : IInputHelper
    {
        public static readonly NoOpInputHelper Instance = new NoOpInputHelper();

        public string ProcessInput(string input)
        {
            return input;
        }
    }
}