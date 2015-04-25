using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;

namespace Registry
{
    public interface IScheduleService
    {
        ICollection<Room> GetRooms();

        ICollection<RecordType> GetRecordTypes();

        ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date);

        RoomWorkingTimeRepository GetRoomsWorkingTime(DateTime date);
    }
}
