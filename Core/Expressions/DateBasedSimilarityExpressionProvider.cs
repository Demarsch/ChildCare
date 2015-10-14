using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Core.Expressions
{
    public abstract class DateBasedSimilarityExpressionProvider<T> : ISimilarityExpressionProvider<T>
    {
        private static readonly Regex DatesRegex = new Regex(@"\d{1,2}[ /\-.]{1}\d{1,2}[ /\-.](\d{2}|\d{4})|\d{1,2} ?\w{3,10} ?(\d{2}|\d{4})");

        protected internal ICollection<DateTime> GetDates(string userInput)
        {
            var result = new HashSet<DateTime>();
            foreach (var match in DatesRegex.Matches(userInput).Cast<Match>().Where(x => x.Success))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(match.Value.Replace("/", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator)
                                           .Replace("-", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator)
                                           .Replace(".", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator),
                                      CultureInfo.CurrentCulture,
                                      DateTimeStyles.None,
                                      out parsedDate))
                {
                    result.Add(parsedDate);
                }
            }
            return result;
        }

        public abstract Expression<Func<T, int>> CreateSimilarityExpression(string searchPattern);
    }
}