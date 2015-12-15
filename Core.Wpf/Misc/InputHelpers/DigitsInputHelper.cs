using System;
using System.Linq;

namespace Core.Wpf.Misc
{
    public sealed class DigitsInputHelper : IInputHelper
    {
        private int maxDigits;

        public DigitsInputHelper()
        {
        }

        public DigitsInputHelper(int maxDigits)
        {
            MaxDigits = maxDigits;
        }

        public int MaxDigits
        {
            get { return maxDigits; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "MaxDigits must not be less than zero");
                }
                maxDigits = value;
            }
        }

        public InputHelperResult ProcessInput(string input)
        {
            input = input ?? string.Empty;
            input = input.Trim();
            if (maxDigits > 0)
            {
                input = input.Substring(0, Math.Min(input.Length, maxDigits));
            }
            var result = new string(input.Where(char.IsDigit).ToArray());
            return new InputHelperResult(result, MaxDigits == 0 || result.Length < MaxDigits);
        }
    }
}