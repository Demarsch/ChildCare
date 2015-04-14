using System;

namespace Core
{
    public class WorkingTime
    {
        private static readonly TimeSpan MaxTime = new TimeSpan(23, 59, 59);

        public WorkingTime(TimeSpan from, TimeSpan to)
        {
            if (from < TimeSpan.Zero || from >= MaxTime)
                throw new ArgumentOutOfRangeException("from");
            if (to < TimeSpan.Zero || to > MaxTime)
                throw new ArgumentOutOfRangeException("to");
            if (from == to && to == TimeSpan.Zero)
                to = MaxTime;
            else if (from == to)
                throw new ArgumentException("'From' value must be less than 'to' value", "from");
            From = from;
            To = to;
        }

        public TimeSpan From { get; private set; }

        public TimeSpan To { get; private set; }
    }
}
