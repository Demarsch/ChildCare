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

        public ILookup<int, WorkingTime> GetRoomsWorkingTime(DateTime date)
        {
            //TODO: replace with real data
            return dataContextProvider.StaticDataContext.GetData<Room>()
                .Select(x => x.Id)
                .AsEnumerable()
                .SelectMany(x => new[]
                {
                    new { Id = x, WorkingTime =  new WorkingTime(date.Date.AddHours(8.0), date.Date.AddHours(12.0)) }, 
                    new { Id = x, WorkingTime =  new WorkingTime(date.Date.AddHours(13.0), date.Date.AddHours(17.0))}
                })
                .ToLookup(x => x.Id, x => x.WorkingTime);
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
    }
}