using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.DTO
{
    public class CommonRecordAssignmentDTO
    {
        public int? ParentVisitId { get; set; }
        public int PersonId { get; set; }
        public int? RoomId { get; set; }
        public int RecordTypeId { get; set; }
        public int ExecutionPlaceId { get; set; }
        public int? RecordPeriodId { get; set; }
        public int UrgentlyId { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime ActualDateTime { get; set; }
        public string RecordTypeName { get; set; }
    }
}
