using System;

namespace DataLib
{
    public partial class ScheduleItem : ICloneable
    {
        public object Clone()
        {
            return new ScheduleItem
            {
                BeginDate = BeginDate,
                DayOfWeek = DayOfWeek,
                EndDate = EndDate,
                EndTime = EndTime,
                Id = Id,
                RecordTypeId = RecordTypeId,
                RoomId = RoomId,
                StartTime = StartTime
            };
        }
    }
}
