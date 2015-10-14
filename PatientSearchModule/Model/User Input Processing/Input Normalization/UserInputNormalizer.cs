using System;
using System.Text;

namespace PatientSearchModule.Model
{
    public class UserInputNormalizer : IUserInputNormalizer
    {
        public string NormalizeUserInput(string userInput)
        {
            userInput = (userInput ?? string.Empty).Trim();
            var result = new StringBuilder(userInput);
            result.Replace(Environment.NewLine, " ");
            result.Replace('\t', ' ');
            var nextCharIsSpace = false;
            for (var index = result.Length - 1; index >= 0; index--)
            {
                if (nextCharIsSpace)
                {
                    result.Remove(index + 1, 1);
                }
                else
                {
                    nextCharIsSpace = result[index] == ' ';
                }
            }
            return result.ToString();
        }
    }
}