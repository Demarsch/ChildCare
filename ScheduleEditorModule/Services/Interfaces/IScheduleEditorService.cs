using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Data;
using Shared.Schedule.Services;

namespace ScheduleEditorModule.Services
{
    public interface IScheduleEditorService : IScheduleServiceBase
    {
        Task SaveScheduleAsync(ICollection<ScheduleItem> newScheduleItems);
    }
}
