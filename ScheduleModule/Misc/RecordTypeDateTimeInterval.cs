using Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data;

namespace ScheduleModule.Misc
{
    public class RecordTypeDateTimeInterval : ITimeInterval
    {
        public Guid MultiAssignRecordTypeGuid { get; set; }

        public RecordType RecordType { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public IList<int> RoomIds { get; set; }

        public Assignment AssignItem { get; set; }

        /// <summary>
        /// Get DateTime from Date property and StartTime property
        /// </summary>
        public DateTime AssignDateTime
        {
            get
            {
                return new DateTime(Date.Year, Date.Month, Date.Day, StartTime.Hours, StartTime.Minutes, StartTime.Seconds);
            }
        }

        public override bool Equals(object obj)
        {
            var obj2 = obj as RecordTypeDateTimeInterval;
            if (obj2 == null)
                return false;
            return this.MultiAssignRecordTypeGuid == obj2.MultiAssignRecordTypeGuid && this.RecordType == obj2.RecordType && this.Date == obj2.Date && this.StartTime == obj2.StartTime && this.EndTime == obj2.EndTime;
        }
        public override int GetHashCode()
        {
            return this.MultiAssignRecordTypeGuid.GetHashCode() + this.Date.GetHashCode() + this.StartTime.GetHashCode() + this.EndTime.GetHashCode();
        }
    }
}
