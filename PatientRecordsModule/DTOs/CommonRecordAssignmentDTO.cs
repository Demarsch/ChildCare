using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientRecordsModule.DTO
{
    public class CommonRecordAssignmentDTO
    {
        public int RecordTypeId { get; set; }
        public int ExecutionPlaceId { get; set; }
        public int? RecordPeriodId { get; set; }
        public int UrgentlyId { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}
