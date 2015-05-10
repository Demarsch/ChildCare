using System;
using Core;
using DataLib;

namespace Registry
{
    public class ScheduleItemViewModel : ITimeInterval
    {
        private readonly ScheduleItem scheduleItem;

        private DateTime date;

        public ScheduleItemViewModel(ScheduleItem scheduleItem, DateTime date)
        {
            if (scheduleItem == null)
                throw new ArgumentNullException("scheduleItem");
            this.scheduleItem = scheduleItem;
            this.date = date.Date;
        }

        public int RoomId { get { return scheduleItem.RoomId; } }

        public int RecordTypeId { get { return scheduleItem.RecordTypeId; } }

        public DateTime StartTime { get { return date.Add(scheduleItem.StartTime); } }

        public DateTime EndTime { get { return date.Add(scheduleItem.EndTime); } }

        TimeSpan ITimeInterval.StartTime { get { return StartTime.TimeOfDay; } }

        TimeSpan ITimeInterval.EndTime { get { return EndTime.TimeOfDay; } }
    }
}
