namespace Core.Wpf.Misc
{
    public class NoOpInputHelper : IInputHelper
    {
        public static readonly NoOpInputHelper Instance = new NoOpInputHelper();

        public InputHelperResult ProcessInput(string input)
        {
            return new InputHelperResult(input, true);
        }
    }
}