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
                .ToLookup(x => x, x => new WorkingTime(TimeSpan.FromHours(8.0), TimeSpan.FromHours(17.0)));
        }

        public ILookup<int, AssignmentDTO> GetRoomsAssignments(DateTime date)
        {
            var endDate = date.Date.AddDays(1.0);
            using (var dataContext = dataContextProvider.GetNewDataContext())
                return dataContext.GetData<Assignment>()
                    .Where(x => x.AssignDateTime >= date.Date && x.AssignDateTime < endDate && !x.CancelUserId.HasValue)
                    .Select(x => new AssignmentDTO
                    {
                        Id = x.Id,
                        AssignDateTime = x.AssignDateTime,
                        IsCompleted = x.RecordId.HasValue && x.Record.IsCompleted,
                        RecordTypeId = x.RecordTypeId,
                        RoomId = x.RoomId
                    })
                    .ToLookup(x => x.RoomId);
        }
    }
}