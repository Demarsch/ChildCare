using System;

namespace Registry
{
    public class ScheduleItemDTO
    {
        public int RoomId { get; set; }

        public int RecordTypeId { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
