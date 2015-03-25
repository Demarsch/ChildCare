using System;

namespace Registry
{
    public class AssignmentDTO
    {
        public DateTime AssignDateTime { get; set; }

        public int RecordTypeId { get; set; }

        public int RoomId { get; set; }

        public bool IsCanceled { get; set; }

        public bool IsCompleted { get; set; }
    }
}
