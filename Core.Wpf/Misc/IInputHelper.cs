namespace Core.Wpf.Misc
{
    public interface IInputHelper
    {
        InputHelperResult ProcessInput(string input);
    }

    public sealed class InputHelperResult
    {
        public InputHelperResult(string result, bool inputCanBeContinued)
        {
            Output = result ?? string.Empty;
            InputCanBeContinued = inputCanBeContinued;
        }

        public string Output { get; private set; }

        public bool InputCanBeContinued { get; private set; }
    }
}
