﻿using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class ScheduleEditorScheduleItemViewModel : ObservableObject
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

        public string RecordType { get { return scheduleItem.RecordTypeId.HasValue ? cacheService.GetItemById<RecordType>(scheduleItem.RecordTypeId.Value).Name : string.Empty; } }

        public DateTime BeginDate { get { return scheduleItem.BeginDate; } }

        public DateTime EndDate { get { return scheduleItem.EndDate; } }

        public TimeSpan StartTime { get { return scheduleItem.StartTime; } }

        public TimeSpan EndTime { get { return scheduleItem.EndTime; } }
    }
}
