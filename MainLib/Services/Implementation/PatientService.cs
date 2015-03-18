using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataLib;

namespace Core
{
    public class PatientService : IPatientService
    {
        public IList<Person> GetPatients(string searchString, int topCount = 0)
        {
            //TODO: check if we get valid result
            var parsedUserInput = ParseUserInput(searchString);
            using (var context = ModelContext.New)
            {
                //TODO: do we need to check for old names?
                var result = context.PersonNames
                    .Where(x => parsedUserInput.Names.Any(y => x.FirstName.StartsWith(y))
                                                 || parsedUserInput.Names.Any(y => x.LastName.StartsWith(y))
                                                 || parsedUserInput.Names.Any(y => x.MiddleName.StartsWith(y))
                                                 || parsedUserInput.Numbers.Any(y => x.Person.Snils == y)
                                                 || parsedUserInput.Numbers.Any(y => x.Person.MedNumber == y));
                //Every match in name, middle name, last name, social number or insurance number gives one point to result
                //Then we sort them by this result descending and show top 5 of them (i.e. those who have more matches will be first on the list)
                //TODO: make this constant a DB parameter
                result = result.OrderByDescending(x => (parsedUserInput.Names.Any(y => x.FirstName.StartsWith(y)) ? 1 : 0)
                                                     + (parsedUserInput.Names.Any(y => x.LastName.StartsWith(y)) ? 1 : 0)
                                                     + (parsedUserInput.Names.Any(y => x.MiddleName.StartsWith(y)) ? 1 : 0)
                                                     + (parsedUserInput.Numbers.Any(y => x.Person.Snils == y) ? 1 : 0)
                                                     + (parsedUserInput.Numbers.Any(y => x.Person.MedNumber == y) ? 1 : 0));
                if (topCount > 0)
                    result = result.Take(topCount);
                return result.Select(x => x.Person)
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
            foreach (var word in userInput.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int number;
                if (int.TryParse(word, out number))
                    numberString.Append(word);
                else
                    result.Names.Add(word);
            }
            if (numberString.Length > 0)
                result.Numbers.Add(numberString.ToString());
            return result;
        }

        internal class ParsedUserInput
        {
            public IList<string> Names { get; private set; }

            public IList<string> Numbers { get; private set; }

            public IList<DateTime> Dates { get; private set; }

            public ParsedUserInput()
            {
                Names = new List<string>();
                Numbers = new List<string>();
                Dates = new List<DateTime>();
            }
        }
    }
}
