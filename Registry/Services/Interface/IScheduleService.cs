using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Registry
{
    public interface IScheduleService
    {
        ICollection<Room> GetRooms();

        ILookup<int, WorkingTime> GetRoomsWorkingTime(DateTime date);

        ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date);
    }
}
