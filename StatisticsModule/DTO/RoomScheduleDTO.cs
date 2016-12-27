using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsModule.DTO
{
    public class RoomScheduleDTO
    {
        public int Id { get; set; }
        public int RecordTypeId { get; set; }
        public string Name { get; set; }
        public DateTime AssignDateTime { get; set; }       
        public int RoomId { get; set; }
        public string Room { get; set; }
        public string PatientFIO { get; set; }
        public string CardNumber { get; set; }
        public int ExecutionPlaceId { get; set; }
        public string ExecutionPlace { get; set; }
        public DateTime ScheduleBeginDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
    }

}
