using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Data.Services;
using Core.Extensions;

namespace Shared.Schedule.Services
{
    public class ScheduleServiceBase : IScheduleServiceBase
    {
        private readonly IDbContextProvider contextProvider;

        public ScheduleServiceBase(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
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
    }
}
