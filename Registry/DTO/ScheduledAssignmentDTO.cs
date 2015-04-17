using System;

namespace Registry
{
    public class ScheduledAssignmentDTO
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string PersonShortName { get; set; }

        public int RecordTypeId { get; set; }

        public DateTime AssignDateTime { get; set; }

        public bool IsCompleted { get; set; }
    }
}
