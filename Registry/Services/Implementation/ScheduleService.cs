using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Registry
{
    public class ScheduleService : IScheduleService
    {
        private readonly IDataContextProvider dataContextProvider;

        public ScheduleService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            this.dataContextProvider = dataContextProvider;
        }

        public ICollection<Room> GetRooms()
        {
            return dataContextProvider.StaticDataContext.GetData<Room>().ToArray();
        }

        public ICollection<RecordType> GetRecordTypes()
        {
            return dataContextProvider.StaticDataContext.GetData<RecordType>().ToArray();
        }

        public ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date)
        {
            var endDate = date.Date.AddDays(1.0);
            using (var dataContext = dataContextProvider.GetNewDataContext())
                return dataContext.GetData<Assignment>()
                    .Where(x => x.AssignDateTime >= date.Date && x.AssignDateTime < endDate && !x.CancelUserId.HasValue)
                    .OrderBy(x => x.AssignDateTime)
                    .Select(x => new ScheduledAssignmentDTO
                    {
                        Id = x.Id,
                        StartTime = x.AssignDateTime,
                        Duration = x.RecordType.Duration,
                        IsCompleted = x.RecordId.HasValue && x.Record.IsCompleted,
                        RecordTypeId = x.RecordTypeId,
                        RoomId = x.RoomId,
                        PersonShortName = x.Person.ShortName
                    })
                    .ToLookup(x => x.RoomId);
        }

        public RoomWorkingTimeRepository GetRoomsWorkingTime(DateTime date)
        {
            date = date.Date;
            var dayOfWeek = (int)date.DayOfWeek;
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                //Only schedule days that are valid for specified date
                return new RoomWorkingTimeRepository(
                        dataContext.GetData<ScheduleDay>()
                       .Where(x => x.DayOfWeek == dayOfWeek && x.BeginDate <= date && x.EndDate >= date)
                       .GroupBy(x => new { x.RoomId, x.RecordTypeId })
                       .Select(x => x.OrderByDescending(y => y.BeginDate).ThenBy(y => y.EndDate).FirstOrDefault())
                       .SelectMany(x => x.ScheduleDayItems)
                       .Select(x => new WorkingTime
                       {
                           RoomId = x.ScheduleDay.RoomId,
                           RecordTypeId = x.ScheduleDay.RecordTypeId,
                           StartTime = x.StartTime,
                           EndTime = x.EndTime
                       }));
            }
        }
    }
}