using System;
using Core.Misc;

namespace Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToFullString(this DateTime thisDate)
        {
            return thisDate.ToString(DateTimeFormats.ShortDateTimeFormat);
        }

        public static DateTime GetQuarterBeginning(this DateTime thisDate)
        {
            var quarterMonth = 0;
            if (thisDate.Month >= 1 && thisDate.Month <= 3)
            {
                quarterMonth = 1;
            }
            if (thisDate.Month >= 4 && thisDate.Month <= 6)
            {
                quarterMonth = 4;
            }
            if (thisDate.Month >= 7 && thisDate.Month <= 9)
            {
                quarterMonth = 7;
            }
            if (thisDate.Month >= 10 && thisDate.Month <= 12)
            {
                quarterMonth = 10;
            }
            return new DateTime(thisDate.Year, quarterMonth, 1);
        }

        public static DateTime GetWeekBegininng(this DateTime thisDate)
        {
            return thisDate.Date.AddDays((int)DayOfWeek.Monday - GetDayOfWeek(thisDate));
        }

        public static DateTime GetWeekEnding(this DateTime thisDate)
        {
            return thisDate.GetWeekBegininng().AddDays(6.0);
        }

        public static DateTime GetMonthBeginning(this DateTime thisDate)
        {
            return new DateTime(thisDate.Year, thisDate.Month, 1);
        }

        public static int GetQuarterIndex(this DateTime thisDate)
        {
            if (thisDate.Month >= 1 && thisDate.Month <= 3)
            {
                return 1;
            }
            if (thisDate.Month >= 4 && thisDate.Month <= 6)
            {
                return 2;
            }
            if (thisDate.Month >= 7 && thisDate.Month <= 9)
            {
                return 3;
            }
            if (thisDate.Month >= 10 && thisDate.Month <= 12)
            {
                return 4;
            }
            throw new ArgumentOutOfRangeException(thisDate.ToShortDateString() + " не относится ни к одному кварталу");
        }

        public static int GetDayOfWeek(this DateTime thisDate)
        {
            return thisDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)thisDate.DayOfWeek;
        }
    }
}