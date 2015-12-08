using System;

namespace Shared.PatientRecords.DTO
{
    public class RecordDTO
    {
        public int Id { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime ActualDateTime { get; set; }
        public string RecordTypeName { get; set; }
        public string FinSourceName { get; set; }
        public string RoomName { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
