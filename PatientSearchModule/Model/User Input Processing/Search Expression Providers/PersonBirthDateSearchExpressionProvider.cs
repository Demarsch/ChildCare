using System;
using System.Linq;
using System.Linq.Expressions;
using Core.Data;
using Core.Expressions;
using Core.Misc;

namespace PatientSearchModule.Model
{
    public class PersonBirthDateSearchExpressionProvider : DateBasedSearchExpressionProvider<Person>
    {
        public override SearchExpression<Person> CreateSearchExpression(string searchPattern)
        {
            var parsedDates = GetDates(searchPattern);
            if (parsedDates.Count == 0)
            {
                return EmptyExpression;
            }
            Expression<Func<Person, int>> expression;
            if (parsedDates.Count == 1)
            {
                var parsedDate = parsedDates.First();
                expression = person => person.BirthDate == parsedDate ? 1 : 0;
            }
            else
            {
                expression = person => parsedDates.Contains(person.BirthDate) ? 1 : 0;
            }
            return new SearchExpression<Person>(parsedDates.Select(x => x.ToString(DateTimeFormats.ShortDateFormat)).ToArray(), expression);
        }
    }
}