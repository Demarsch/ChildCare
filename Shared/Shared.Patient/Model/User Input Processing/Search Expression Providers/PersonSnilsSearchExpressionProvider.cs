using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core.Data;
using Core.Expressions;
using Core.Extensions;

namespace Shared.Patient.Model
{
    public class PersonSnilsSearchExpressionProvider : SearchExpressionProvider<Person>
    {
        private static readonly Regex SnilsRegex = new Regex(@"\d{3} ?\d{3} ?\d{3}-?\d{2}");
        
        internal ICollection<string> GetSnilsCollection(string userInput)
        {
            var snilsCollection = new HashSet<string>(SnilsRegex
                                                          .Matches(userInput)
                                                          .Cast<Match>()
                                                          .Where(x => x.Success)
                                                          .Select(x => x.Value),
                                                      StringComparer.CurrentCultureIgnoreCase);
            snilsCollection.Select(Person.DelimitizeSnils).ToArray().ForEach(x => snilsCollection.Add(x));
            return snilsCollection;
        }

        public override SearchExpression<Person> CreateSearchExpression(string searchPattern)
        {
            var parsedSnilsCollection = GetSnilsCollection(searchPattern);
            if (parsedSnilsCollection.Count == 0)
            {
                return EmptyExpression;
            }
            Expression<Func<Person, int>> expression;
            if (parsedSnilsCollection.Count == 1)
            {
                var parsedSnils = parsedSnilsCollection.First();
                expression = person => person.Snils == parsedSnils ? 1 : 0;
            }
            else
            {
                expression = person => parsedSnilsCollection.Contains(person.Snils) ? 1 : 0;
            }
            return new SearchExpression<Person>(parsedSnilsCollection, expression);
        }
    }
}
