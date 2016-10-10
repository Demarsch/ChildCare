using Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data;

namespace ScheduleModule.Misc
{
    public class SelectedTimesEventArg : EventArgs
    {
        public ITimeInterval TimeInterval { get; set; }
        public DateTime Date { get; set; }
        public RecordType RecordType { get; set; }
        public bool IsAdded { get; set; }
    }
}
