using System;

namespace ScheduleModule.ViewModels
{
    public class TimeTickerViewModel
    {
        public TimeTickerViewModel(DateTime time)
        {
            StartTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
            EndTime = StartTime.AddHours(1.0);
        }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }
    }
}
