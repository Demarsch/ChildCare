using System;

namespace PatientRecordsModule.DTO
{
    public class VisitTemplateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Contract { get; set; }
        public string FinancingSource { get; set; }
        public string Urgently { get; set; }
    }
}
