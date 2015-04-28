using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public ICollection<ScheduleItemDTO> GetRoomsWorkingTime(DateTime date)
        {
            date = date.Date;
            var dayOfWeek = (int)date.DayOfWeek;
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                var result = dataContext.GetData<ScheduleItem>().Where(x => x.BeginDate <= date && x.EndDate >= date && x.DayOfWeek == dayOfWeek);
                return result.GroupBy(x => new { x.RoomId, x.RecordTypeId })
                    .SelectMany(x => x.GroupBy(y => new { y.BeginDate, y.EndDate }).OrderByDescending(y => y.Key.BeginDate).ThenBy(y => y.Key.EndDate).FirstOrDefault())
                    .Select(x => new ScheduleItemDTO
                    {
                        RoomId = x.RoomId, 
                        RecordTypeId = x.RecordTypeId,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime
                    })
                    .ToArray();
            }
        }
    }
}