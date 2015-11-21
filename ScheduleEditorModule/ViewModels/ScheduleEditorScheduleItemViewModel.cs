using System;
using Core.Data;
using Core.Misc;
using Core.Services;
using Prism.Mvvm;

namespace ScheduleEditorModule.ViewModels
{
    public class ScheduleEditorScheduleItemViewModel : BindableBase, ITimeInterval
    {
        private readonly ScheduleItem scheduleItem;

        private readonly ICacheService cacheService;

        public ScheduleEditorScheduleItemViewModel(ScheduleItem scheduleItem, ICacheService cacheService)
        {
            if (scheduleItem == null)
            {
                throw new ArgumentNullException("scheduleItem");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            this.scheduleItem = scheduleItem;
        }

        public string RecordTypeName { get { return scheduleItem.RecordTypeId.HasValue ? cacheService.GetItemById<RecordType>(scheduleItem.RecordTypeId.Value).Name : string.Empty; } }

        public int RecordTypeId { get { return scheduleItem.RecordTypeId.GetValueOrDefault(); } }

        public DateTime BeginDate { get { return scheduleItem.BeginDate; } }

        public DateTime EndDate { get { return scheduleItem.EndDate; } }

        public TimeSpan StartTime { get { return scheduleItem.StartTime; } }

        public TimeSpan EndTime { get { return scheduleItem.EndTime; } }
        
        public int Id { get { return scheduleItem.Id; } }

        public ScheduleItem GetScheduleItem()
        {
            return scheduleItem;
        }
    }
}
