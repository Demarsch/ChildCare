using System;
using System.Drawing;

namespace Shared.PatientRecords.DTO
{
    public class AnalyseParameterDTO
    {
        public int Id { get; set; }
        public int ParameterRecordTypeId { get; set; }
        public string Name { get; set; }
        public string UnitName { get; set; }
        public string Result { get; set; }
        public double? RefMin { get; set; }
        public double? RefMax { get; set; }
        public string Details { get; set; }
        public Color Background { get; set; }
    }
}

