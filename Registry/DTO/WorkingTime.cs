using System;

namespace Registry
{
    public class WorkingTime
    {
        public int RoomId { get; set; }

        public int RecordTypeId { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
