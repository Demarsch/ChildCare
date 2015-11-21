using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Services;
using Core.Extensions;

namespace ScheduleEditorModule.Services
{
    public class ScheduleEditorService : IScheduleEditorService
    {
        private readonly IDbContextProvider contextProvider;

        public ScheduleEditorService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IEnumerable<ScheduleItem> GetRoomsWeeklyWorkingTime(DateTime date)
        {
            var @from = date.GetWeekBegininng();
            var to = date.GetWeekEnding();
            @from = @from.Date;
            to = to.Date;
            using (var context = contextProvider.CreateNewContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                var currentDate = @from;
                var result = new List<ScheduleItem>();
                while (currentDate <= to)
                {
                    var dayOfWeek = currentDate.GetDayOfWeek();
                    var currentResult = context.Set<ScheduleItem>()
                                                   .Where(x => x.BeginDate <= currentDate && x.EndDate >= currentDate && x.DayOfWeek == dayOfWeek);
                    result.AddRange(currentResult.GroupBy(x => x.RoomId)
                                                 .SelectMany(x => x.GroupBy(y => new { y.BeginDate, y.EndDate }).OrderByDescending(y => y.Key.BeginDate).ThenBy(y => y.Key.EndDate).FirstOrDefault()));
                    currentDate = currentDate.AddDays(1.0);
                }
                return result;
            }
        }

        public async Task SaveScheduleAsync(ICollection<ScheduleItem> newScheduleItems)
        {
            using (var dataContext = contextProvider.CreateNewContext())
            {
                var itemsToDelete = new List<int>();
                var itemsByRoomAndDayOfWeek = newScheduleItems.ToLookup(x => new { x.RoomId, x.DayOfWeek, x.BeginDate, x.EndDate });
                foreach (var itemGroup in itemsByRoomAndDayOfWeek)
                {
                    var @group = itemGroup;
                    itemsToDelete.AddRange(await dataContext.Set<ScheduleItem>()
                                                            .Where(x => x.RoomId == @group.Key.RoomId
                                                                        && x.DayOfWeek == @group.Key.DayOfWeek
                                                                        && x.BeginDate == @group.Key.BeginDate
                                                                        && (x.EndDate == @group.Key.EndDate || x.EndDate == x.BeginDate))
                                                            .Select(x => x.Id)
                                                            .ToArrayAsync());
                }
                dataContext.Set<ScheduleItem>().AddRange(newScheduleItems);
                foreach (var itemToDelete in itemsToDelete)
                {
                    dataContext.Entry(new ScheduleItem { Id = itemToDelete }).State = EntityState.Deleted;
                }
                await dataContext.SaveChangesAsync();
            }
        }

        public void SaveSchedule(ICollection<ScheduleItem> newScheduleItems)
        {
            
        }
    }
}