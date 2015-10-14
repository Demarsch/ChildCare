using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core.Data;
using Core.Expressions;

namespace PatientSearchModule.Model
{
    public class PersonMedNumberSearchExpressionProvider : ISearchExpressionProvider<Person>
    {
        private static readonly Regex MedNumberRegex = new Regex(@"\d{13}");

        internal ICollection<string> GetMedNumbersCollection(string userInput)
        {
            return new HashSet<string>(MedNumberRegex
                                           .Matches(userInput)
                                           .Cast<Match>()
                                           .Where(x => x.Success)
                                           .Select(x => x.Value),
                                       StringComparer.CurrentCultureIgnoreCase);
        }

        public Expression<Func<Person, int>> CreateSearchExpression(string searchPattern)
        {
            var parsedMedNumbersCollection = GetMedNumbersCollection(searchPattern);
            if (parsedMedNumbersCollection.Count == 0)
            {
                return null;
            }
            if (parsedMedNumbersCollection.Count == 1)
            {
                var parsedMedNumber = parsedMedNumbersCollection.First();
                return person => person.MedNumber == parsedMedNumber ? 1 : 0;
            }
            return person => parsedMedNumbersCollection.Contains(person.MedNumber) ? 1 : 0;
        }
    }
}
