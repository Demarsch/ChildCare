using System;

namespace Core
{
    public class AssignmentScheduleDTO
    {
        public int Id { get; set; }

        public DateTime AssignDateTime { get; set; }

        public int RecordTypeId { get; set; }

        public int RoomId { get; set; }

        public int Duration { get; set; }

        public bool IsCanceled { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsTemporary { get; set; }
    }
}
