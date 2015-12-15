using System.Linq;
using System.Text;
using Core.Extensions;
using Core.Wpf.Misc;

namespace PatientInfoModule.Misc
{
    public class RussianBirthCertificateSeriesInputHelper : IInputHelper
    {
        public InputHelperResult ProcessInput(string input)
        {
            input = input ?? string.Empty;
            input = input.Trim();
            var result = new StringBuilder();
            var romanNumber = input.TakeWhile(CharExtensions.IsRomanNumber).Select(char.ToUpper).ToArray();
            var delimiterChar = input.SkipWhile(CharExtensions.IsRomanNumber).Take(1).FirstOrDefault();
            var hasDelimiter = delimiterChar != default(char) && (delimiterChar == ' ' || delimiterChar == '-' || delimiterChar == '_');
            var russianLetters = input.SkipWhile(CharExtensions.IsRomanNumber)
                                      .Skip(hasDelimiter ? 1 : 0)
                                      .TakeWhile(CharExtensions.IsRussianLetter)
                                      .Take(2)
                                      .Select(char.ToUpper)
                                      .ToArray();
            if (romanNumber.Length != 0)
            {
                result.Append(romanNumber);
                if (hasDelimiter)
                {
                    result.Append('-');
                }
                if (russianLetters.Length > 0)
                {
                    if (!hasDelimiter)
                    {
                        result.Append('-');
                    }
                    result.Append(russianLetters);
                }
            }
            return new InputHelperResult(result.ToString(), russianLetters.Length < 2);
        }
    }
}
