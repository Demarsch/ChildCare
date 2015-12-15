using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Core.Extensions;

namespace Core.Wpf.Misc
{
    public class ShortDateFormatInputHelper : IInputHelper
    {
        private const int DayDigitsStartIndex = 0;

        private const int MonthDigitsStartIndex = 2;

        private const int YearDigitsStartIndex = 4;

        private const int DayDigits = 2;

        private const int MonthDigits = 2;

        private const int YearDigits = 4;

        private const int TotalDigitsCount = 8;

        public bool BlockInputCompletion { get; set; }

        private const char Separator = '.';

        public InputHelperResult ProcessInput(string input)
        {
            Debug.WriteLine("Processing " + input);
            input = input ?? string.Empty;
            input = input.Trim();
            var digits = input.Where(char.IsDigit).ToArray();
            var hasDayMonthSeparator = input.Length > MonthDigitsStartIndex && input[MonthDigitsStartIndex].IsDateSeparator();
            var hasMonthYearSeparator = input.Length > YearDigitsStartIndex + 1 && input[YearDigitsStartIndex + 1].IsDateSeparator();
            var result = new StringBuilder();
            if (digits.Length > DayDigitsStartIndex)
            {
                result.Append(digits, DayDigitsStartIndex, Math.Min(DayDigits, digits.Length));
            }
            if (hasDayMonthSeparator)
            {
                result.Append(Separator);
            }
            if (digits.Length > MonthDigitsStartIndex)
            {
                if (!hasDayMonthSeparator)
                {
                    result.Append(Separator);
                }
                result.Append(digits, MonthDigitsStartIndex, Math.Min(MonthDigits, digits.Length - MonthDigitsStartIndex));
            }
            if (hasMonthYearSeparator)
            {
                result.Append(Separator);
            }
            if (digits.Length > YearDigitsStartIndex)
            {
                if (!hasMonthYearSeparator)
                {
                    result.Append(Separator);
                }
                result.Append(digits, YearDigitsStartIndex, Math.Min(YearDigits, digits.Length - YearDigitsStartIndex));
            }
            return new InputHelperResult(result.ToString(), digits.Length < TotalDigitsCount || BlockInputCompletion);
        }
    }
}
