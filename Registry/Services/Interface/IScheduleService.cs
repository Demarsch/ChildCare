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

        ICollection<RecordType> GetRecordTypes();

        ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date);

        ICollection<ScheduleItemDTO> GetRoomsWorkingTime(DateTime date);

        TimeIntervalCollection GetFreeTimes(IEnumerable<ITimeInterval> workingTime, IEnumerable<ITimeInterval> occupiedTime, TimeSpan minimumDuration, TimeSpan generalDuration); 
    }
}
