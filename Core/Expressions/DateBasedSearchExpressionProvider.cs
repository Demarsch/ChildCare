using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Expressions
{
    public abstract class DateBasedSearchExpressionProvider<T> : SearchExpressionProvider<T>
    {
        //private static readonly Regex DatesRegex = new Regex(@"((0?[13578]|10|12)(-|\/|.)(([1-9])|(0[1-9])|([12])([0-9]?)|(3[01]?))(-|\/|.)((19)([2-9])(\d{1})|(20)([01])(\d{1})|([8901])(\d{1}))|(0?[2469]|11)(-|\/|.)(([1-9])|(0[1-9])|([12])([0-9]?)|(3[0]?))(-|\/|.)((19)([2-9])(\d{1})|(20)([01])(\d{1})|([8901])(\d{1})))");

        private static readonly Regex DatesRegex = new Regex(@"[0-9]{1,2}[-/\. ][0-9]{1,2}[-/\. ][0-9]{2,4}");

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
    }
}