using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataLib;

namespace Registry
{
    //TODO: extract interface to be able to mock it
    public class MainService
    {
        public IList<Person> GetPeople(string userInput)
        {
            //TODO: check if we get valid result
            var parsedUserInput = ParseUserInput(userInput);
            using (var context = ModelContext.New)
            {
                //TODO: do we need to check for old names?
                var result = context.PersonNames
                    .Where(x => !x.EndDateTime.HasValue)
                    .Where(x =>parsedUserInput.Names.Contains(x.FirstName)
                                                 || parsedUserInput.Names.Contains(x.MiddleName)
                                                 || parsedUserInput.Names.Contains(x.LastName)
                                                 || parsedUserInput.Number == x.Person.Snils
                                                 || parsedUserInput.Number == x.Person.MedNumber);
                //Every match in name, middle name, last name, social number or insurance number gives one point to result
                //Then we sort them by this result descending and show top 5 of them (i.e. those who have more matches will be first on the list)
                //TODO: make this constant a DB parameter
                return result.OrderByDescending(x => (parsedUserInput.Names.Contains(x.FirstName) ? 1 : 0)
                                                     + (parsedUserInput.Names.Contains(x.MiddleName) ? 1 : 0)
                                                     + (parsedUserInput.Names.Contains(x.LastName) ? 1 : 0)
                                                     + (parsedUserInput.Number == x.Person.Snils ? 1 : 0)
                                                     + (parsedUserInput.Number == x.Person.MedNumber ? 1 : 0))
                                                     .Take(5)
                                                     .Select(x => x.Person)
                                                     .ToArray();
            }
        }
        //TODO: this is a draft version
        //TODO: to be tested
        internal ParsedUserInput ParseUserInput(string userInput)
        {
            var sanitizedInput = new StringBuilder(userInput.Length);
            foreach (var symbol in userInput)
                if (char.IsLetterOrDigit(symbol) || symbol == '-' || symbol == ' ')
                    sanitizedInput.Append(symbol);
            userInput = sanitizedInput.ToString();
            var result = new ParsedUserInput();
            var numberString = new StringBuilder();
            foreach (var word in userInput.Split(new [] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int number;
                if (int.TryParse(word, out number))
                    numberString.Append(word);
                else
                    result.Names.Add(word);
            }
            if (numberString.Length > 0)
                result.Number = numberString.ToString();
            return result;
        }

        internal class ParsedUserInput
        {
            public IList<string> Names { get; private set; }

            public string Number { get; set; }

            public ParsedUserInput()
            {
                Names = new List<string>();
            }
        }
    }
}
