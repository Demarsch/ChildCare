using System;

namespace PatientRecordsModule.DTO
{
    public class VisitDTO
    {
        public int Id { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string FinSource { get; set; }
        public string Name { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime ActualDateTime { get; set; }
    }
}
