using System.Linq;
using System.Text;
using Core.Data;
using Core.Wpf.Misc;

namespace PatientInfoModule.Misc
{
    public class SnilsInputHelper : IInputHelper
    {
        private const int FirstDashIndex = 3;

        private const int SecondDashIndex = 7;

        private const int SpaceIndex = 11;

        public InputHelperResult ProcessInput(string input)
        {
            input = new string(input.Where(char.IsDigit).ToArray());
            var properSnils = new StringBuilder();
            var snilsIndex = 0;
            var textIndex = 0;
            while (textIndex < input.Length && snilsIndex < Person.FullSnilsLength)
            {
                if (snilsIndex == SpaceIndex)
                {
                    properSnils.Append(' ');
                    textIndex--;
                }
                else if (snilsIndex == FirstDashIndex || snilsIndex == SecondDashIndex)
                {
                    properSnils.Append('-');
                    textIndex--;
                }
                else
                {
                    properSnils.Append(input[textIndex]);
                }
                textIndex++;
                snilsIndex++;
            }
            return new InputHelperResult(properSnils.ToString(), properSnils.Length < Person.FullSnilsLength);
        }
    }
}
