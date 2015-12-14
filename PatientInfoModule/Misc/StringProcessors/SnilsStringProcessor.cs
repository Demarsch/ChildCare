﻿using System.Linq;
using System.Text;
using Core.Wpf.Misc;

namespace PatientInfoModule.Misc
{
    public class SnilsStringProcessor : IStringProcessor
    {
        private const int FullSnilsLength = 14;

        private const int FirstDashIndex = 3;

        private const int SecondDashIndex = 7;

        private const int SpaceIndex = 11;

        public string ProcessString(string input)
        {
            input = new string(input.Where(char.IsDigit).ToArray());
            var properSnils = new StringBuilder();
            var snilsIndex = 0;
            var textIndex = 0;
            while (textIndex < input.Length && snilsIndex < FullSnilsLength)
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
            return properSnils.ToString();
        }
    }
}
