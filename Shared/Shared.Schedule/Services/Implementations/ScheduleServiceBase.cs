using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Data.Services;
using Core.Extensions;
using Core.Services;

namespace Shared.Schedule.Services
{
    public class ScheduleServiceBase : IScheduleServiceBase
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ICacheService cacheService;

        public ScheduleServiceBase(IDbContextProvider contextProvider, ICacheService cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.contextProvider = contextProvider;
            this.cacheService = cacheService;
        }

        public IEnumerable<ScheduleItem> GetRoomsWorkingTimeForDay(DateTime date)
        {
            return GetRoomsWorkingTimeImpl(date, date);
        }

        public IEnumerable<ScheduleItem> GetRoomsWorkingTimeForWeek(DateTime date)
        {
            return GetRoomsWorkingTimeImpl(date.GetWeekBegininng(), date.GetWeekEnding());
        }
    
        private IEnumerable<ScheduleItem> GetRoomsWorkingTimeImpl(DateTime from, DateTime to)
        {
            from = from.Date;
            to = to.Date;
            using (var dataContext = contextProvider.CreateNewContext())
            {
                dataContext.Configuration.ProxyCreationEnabled = false;
                var currentDate = from;
                var result = new List<ScheduleItem>();
                while (currentDate <= to)
                {
                    var dayOfWeek = currentDate.GetDayOfWeek();
                    var currentResult = dataContext.Set<ScheduleItem>()
                                                   .Where(x => x.BeginDate <= currentDate && x.EndDate >= currentDate && x.DayOfWeek == dayOfWeek);
                    result.AddRange(currentResult.GroupBy(x => x.RoomId)
                                                 .SelectMany(x => x.GroupBy(y => new { y.BeginDate, y.EndDate }).OrderByDescending(y => y.Key.BeginDate).ThenBy(y => y.Key.EndDate).FirstOrDefault()));
                    currentDate = currentDate.AddDays(1.0);
                }
                return result;
            }
        }

        public IEnumerable<Room> GetRooms()
        {
            return cacheService.GetItems<Room>();
        }

        public IEnumerable<RecordType> GetRecordTypes()
        {
            return cacheService.GetItems<RecordType>().Where(x => x.ParentId == null && x.IsRecord)
                .Select(CopyRecordType)
                .Where(x => x != null)
                .ToArray();
        }

        private RecordType CopyRecordType(RecordType recordType)
        {
            if (recordType.Assignable == null)
            {
                return null;
            }
            var result = new RecordType { Id = recordType.Id, Name = recordType.Name, Assignable = recordType.Assignable };
            var children = recordType.RecordTypes1.Select(CopyRecordType).Where(x => x != null).ToList();
            if (children.Count == 0 && result.Assignable != true)
            {
                return null;
            }
            result.RecordTypes1 = children.Count == 0 ? null : children;
            foreach (var childRecortType in children)
            {
                childRecortType.RecordType1 = result;
            }
            return result;
        }
    }
}
