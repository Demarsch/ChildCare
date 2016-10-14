using Core.Misc;
using ScheduleModule.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleModule.DTO
{
    public class ScheduleDateTimesDTO
    {
        public DateTime Date { get; set; }

        public IEnumerable<RecordTypeDateTimeInterval> Times { get; set; }
    }
}
