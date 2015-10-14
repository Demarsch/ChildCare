using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core.Data;
using Core.Expressions;

namespace PatientSearchModule.Model
{
    public class PersonSnilsSearchExpressionProvider : ISearchExpressionProvider<Person>
    {
        private static readonly Regex SnilsRegex = new Regex(@"\d{3} ?\d{3} ?\d{3}-?\d{2}");
        
        internal ICollection<string> GetSnilsCollection(string userInput)
        {
            return new HashSet<string>(SnilsRegex
                                           .Matches(userInput)
                                           .Cast<Match>()
                                           .Where(x => x.Success)
                                           .Select(x => x.Value),
                                       StringComparer.CurrentCultureIgnoreCase);
        }

        public Expression<Func<Person, int>> CreateSearchExpression(string searchPattern)
        {
            var parsedSnilsCollection = GetSnilsCollection(searchPattern);
            if (parsedSnilsCollection.Count == 0)
            {
                return null;
            }
            if (parsedSnilsCollection.Count == 1)
            {
                var parsedSnils = parsedSnilsCollection.First();
                return person => person.Snils == parsedSnils ? 1 : 0;
            }
            return person => parsedSnilsCollection.Contains(person.Snils) ? 1 : 0;
        }
    }
}
