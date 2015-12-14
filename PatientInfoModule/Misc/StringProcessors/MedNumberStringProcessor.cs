using System;
using System.Linq;
using Core.Wpf.Misc;

namespace PatientInfoModule.Misc
{
    public class MedNumberStringProcessor : IStringProcessor
    {
        private const int FullMedNumberLength = 16;

        public string ProcessString(string input)
        {
            input = input ?? string.Empty;
            input = input.Trim();
            input = input.Substring(0, Math.Min(input.Length, FullMedNumberLength));
            return new string(input.Where(char.IsDigit).ToArray());
        }
    }
}
