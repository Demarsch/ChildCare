using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyclinicModule.DTO
{
    public class RecordDTO
    {
        public int? AssignmentId { get; set; }
        public int? RecordId { get; set; }
        public int PersonId { get; set; }
        public DateTime ResultDate { get; set; }
        public string PersonBirthYear { get; set; }
        public string PatientFIO { get; set; }
        public bool IsCompleted { get; set; }
    }
}
