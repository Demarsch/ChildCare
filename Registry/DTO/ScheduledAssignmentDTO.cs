using System;
using Core;
using DataLib;

namespace Registry
{
    public class ScheduledAssignmentDTO : ITimeInterval
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string PersonShortName { get; set; }

        public int RecordTypeId { get; set; }

        public DateTime StartTime { get; set; }

        public int Duration { get; set; }

        public DateTime EndTime { get { return StartTime.AddMinutes(Duration); }}

        public bool IsCompleted { get; set; }

        public bool IsTemporary { get; set; }

        public int AssignUserId { get; set; }

        public string Note { get; set; }

        public int FinancingSourceId { get; set; }

        TimeSpan ITimeInterval.StartTime
        {
            get { return StartTime.TimeOfDay; }
        }

        TimeSpan ITimeInterval.EndTime
        {
            get { return EndTime.TimeOfDay; }
        }
    }
}
