using System;
using System.Windows.Input;

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

        public string DayOfWeekProperName
        {
            get
            {
                switch (DayOfWeek)
                {
                    case 1:
                        return "понедельник";
                    case 2:
                        return "вторник";
                    case 3:
                        return "среду";
                    case 4:
                        return "четверг";
                    case 5:
                        return "пятницу";
                    case 6:
                        return "субботу";
                    case 7:
                        return "воскресенье";
                }
                throw new NotSupportedException();
            }
        }

        public ICommand CloseDayThisWeekCommand { get; set; }

        public ICommand CloseDayCommand { get; set; }
    }
}