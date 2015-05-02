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

        IEnumerable<ITimeInterval> GetAvailableTimeIntervals(IEnumerable<ITimeInterval> workingTime, IEnumerable<ITimeInterval> occupiedTime, int nominalDurationInMinutes, int minimumDurationInMinutes);

        void SaveAssignment(Assignment assignment);
    }
}
