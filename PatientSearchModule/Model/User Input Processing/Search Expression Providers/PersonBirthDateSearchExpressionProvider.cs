using System;
using System.Linq;
using System.Linq.Expressions;
using Core.Data;
using Core.Expressions;

namespace PatientSearchModule.Model
{
    public class PersonBirthDateSearchExpressionProvider : DateBasedSearchExpressionProvider<Person>
    {
        public override Expression<Func<Person, int>> CreateSearchExpression(string searchPattern)
        {
            var parsedDates = GetDates(searchPattern);
            if (parsedDates.Count == 0)
            {
                return null;
            }
            if (parsedDates.Count == 1)
            {
                var parsedDate = parsedDates.First();
                return person => person.BirthDate == parsedDate ? 1 : 0;
            }
            return person => parsedDates.Contains(person.BirthDate) ? 1 : 0;
        }
    }
}