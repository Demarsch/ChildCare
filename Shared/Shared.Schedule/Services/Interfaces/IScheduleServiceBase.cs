using System;
using System.Collections.Generic;
using Core.Data;

namespace Shared.Schedule.Services
{
    public interface IScheduleServiceBase
    {
        IEnumerable<ScheduleItem> GetRoomsWorkingTimeForDay(DateTime date);

        IEnumerable<ScheduleItem> GetRoomsWorkingTimeForWeek(DateTime date);

        IEnumerable<Room> GetRooms();

        IEnumerable<RecordType> GetRecordTypes();
    }
}
