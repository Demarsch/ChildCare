using System;

namespace Registry
{
    public class ScheduledAssignmentDTO
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string PersonShortName { get; set; }

        public int RecordTypeId { get; set; }

        public DateTime StartTime { get; set; }

        public int Duration { get; set; }

        public DateTime EndTime { get { return StartTime.AddMinutes(Duration); }}

        public bool IsCompleted { get; set; }
    }
}
