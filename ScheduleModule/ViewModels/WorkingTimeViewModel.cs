using System;
using Core.Data;
using Core.Misc;
using Core.Services;

namespace ScheduleModule.ViewModels
{
    public class WorkingTimeViewModel : ITimeInterval
    {
        private readonly ScheduleItem scheduleItem;

        private DateTime date;

        public WorkingTimeViewModel(ScheduleItem scheduleItem, DateTime date, ICacheService cacheService)
        {
            if (scheduleItem == null)
            {
                throw new ArgumentNullException("scheduleItem");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.scheduleItem = scheduleItem;
            this.date = date.Date;
            Room = cacheService.GetItemById<Room>(scheduleItem.RoomId);
            RecordType = scheduleItem.RecordTypeId == null ? null : cacheService.GetItemById<RecordType>(scheduleItem.RecordTypeId.Value);
        }

        public Room Room { get; private set; }

        public RecordType RecordType { get; set; }

        public DateTime StartTime { get { return date.Add(scheduleItem.StartTime); } }

        public DateTime EndTime { get { return date.Add(scheduleItem.EndTime); } }

        TimeSpan ITimeInterval.StartTime { get { return StartTime.TimeOfDay; } }

        TimeSpan ITimeInterval.EndTime { get { return EndTime.TimeOfDay; } }
    }
}
