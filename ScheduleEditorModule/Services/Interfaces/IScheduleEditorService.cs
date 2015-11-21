using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Data;

namespace ScheduleEditorModule.Services
{
    public interface IScheduleEditorService
    {
        IEnumerable<ScheduleItem> GetRoomsWeeklyWorkingTime(DateTime date);

        Task SaveScheduleAsync(ICollection<ScheduleItem> newScheduleItems);
    }
}
