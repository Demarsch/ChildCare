using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Services;
using Core.Services;
using Shared.Schedule.Services;

namespace ScheduleEditorModule.Services
{
    public class ScheduleEditorService : ScheduleServiceBase, IScheduleEditorService
    {
        private readonly IDbContextProvider contextProvider;

        public ScheduleEditorService(IDbContextProvider contextProvider, ICacheService cacheService)
            : base(contextProvider, cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
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
    }
}