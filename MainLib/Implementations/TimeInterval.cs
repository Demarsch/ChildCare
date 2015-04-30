using System;

namespace Core
{
    public class TimeInterval : ITimeInterval
    {
        public TimeInterval(TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime > endTime)
                throw new ArgumentOutOfRangeException("startTime", "Start time must be less than or equal to end time");
            StartTime = startTime;
            EndTime = endTime;
        }

        public TimeInterval(double startHour, double endHour) : this(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour)) { }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public override string ToString()
        {
            return string.Format("{0:hh\\:mm} - {1:hh\\:mm}", StartTime, EndTime);
        }
    }
}
