using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core.Data;
using Core.Expressions;

namespace PatientSearchModule.Model
{
    public class PersonIdentityDocumentNumberSearchExpressionProvider : ISearchExpressionProvider<Person>
    {
        private static readonly Regex DocumentNumbersRegex = new Regex(@"[\w\d]{1,6} {1}\d{1, 10}");

        public Expression<Func<Person, int>> CreateSearchExpression(string searchPattern)
        {
            var parsedIdentityNumbers = GetIdentityNumbers(searchPattern);
            if (parsedIdentityNumbers.Count == 0)
            {
                return null;
            }
            if (parsedIdentityNumbers.Count == 1)
            {
                var parsedIdentityNumber = parsedIdentityNumbers.First();
                return person => person
                                     .PersonIdentityDocuments
                                     .Any(x => x.Series + " " + x.Number == parsedIdentityNumber) ? 1 : 0;
            }
            return person => person
                                 .PersonIdentityDocuments
                                 .Any(x => parsedIdentityNumbers.Contains(x.Series + " " + x.Number)) ? 1 : 0;
        }

        internal ICollection<string> GetIdentityNumbers(string userInput)
        {
            return new HashSet<string>(DocumentNumbersRegex
                                           .Matches(userInput.Replace('-', ' '))
                                           .Cast<Match>()
                                           .Where(x => x.Success)
                                           .Select(x => x.Value),
                                       StringComparer.CurrentCultureIgnoreCase);
        }
    }
}