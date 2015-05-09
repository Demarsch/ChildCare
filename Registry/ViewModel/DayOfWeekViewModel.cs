using System;

namespace Registry
{
    public class DayOfWeekViewModel
    {
        public DayOfWeekViewModel(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; private set; }

        public int DayOfWeek
        {
            get { return Date.DayOfWeek == System.DayOfWeek.Sunday ? 7 : (int)Date.DayOfWeek; }
        }
    }
}