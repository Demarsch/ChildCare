using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core.Data;
using Core.Expressions;

namespace PatientSearchModule.Model
{
    public class PersonNamesSimilarityExpressionProvider : ISimilarityExpressionProvider<Person>
    {
        private static readonly Regex NamesRegex = new Regex(@"\w+|\w+-{1}\w+");

        public Expression<Func<Person, int>> CreateSimilarityExpression(string searchPattern)
        {
            var parsedNames = GetNames(searchPattern);
            if (parsedNames.Count == 0)
                return null;
            if (parsedNames.Count == 1)
            {
                var parsedName = parsedNames.First();
                return person => person
                    .PersonNames
                    .Where(x => x.ChangeNameReason == null || x.ChangeNameReason.NeedCreateNewPersonName)
                    .Max(x => (x.FirstName.StartsWith(parsedName) ? 1 : 0)
                              + (x.MiddleName.StartsWith(parsedName) ? 1 : 0)
                              + (x.LastName.StartsWith(parsedName) ? 1 : 0));
            }
            return person => person
                .PersonNames
                .Where(x => x.ChangeNameReason == null || x.ChangeNameReason.NeedCreateNewPersonName)
                .Max(x => (parsedNames.Any(y => x.FirstName.StartsWith(y)) ? 1 : 0)
                          + (parsedNames.Any(y => x.MiddleName.StartsWith(y)) ? 1 : 0)
                          + (parsedNames.Any(y => x.LastName.StartsWith(y)) ? 1 : 0));
        }

        internal ICollection<string> GetNames(string userInput)
        {
            return new HashSet<string>(NamesRegex
                .Matches(userInput)
                .Cast<Match>()
                .Where(x => x.Success)
                .Select(x => x.Value),
                StringComparer.CurrentCultureIgnoreCase);
        }
    }
}