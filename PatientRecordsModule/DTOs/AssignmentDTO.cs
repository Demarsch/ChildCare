using System;

namespace PatientRecordsModule.DTO
{
    public class AssignmentDTO
    {
        public int Id { get; set; }
        public string RecordTypeName { get; set; }
        public DateTime ActualDateTime { get; set; }
        public string RoomName { get; set; }
        public string FinancingSourceName { get; set; }
    }
}
