using System;

namespace Core
{
    public class WorkingTime
    {
        public WorkingTime(DateTime from, DateTime to)
        {
           if (from >= to)
                throw new ArgumentException("'From' value must be less than 'to' value", "from");
            From = from;
            To = to;
        }

        public DateTime From { get; private set; }

        public DateTime To { get; private set; }
    }
}
