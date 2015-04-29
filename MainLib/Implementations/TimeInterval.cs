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

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
