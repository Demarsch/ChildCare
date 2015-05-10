using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class ScheduleEditorRecordTypeViewModel : ObservableObject
    {
        private readonly ICacheService cacheService;

        private readonly int recordTypeId;

        public ScheduleEditorRecordTypeViewModel(int recordTypeId, IEnumerable<ITimeInterval> times, ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (times == null)
            {
                throw new ArgumentNullException("times");
            }
            Time = string.Join(", ", times.OrderBy(x => x.StartTime).Select(x => string.Format("{0:hh\\:mm}-{1:hh\\:mm}", x.StartTime, x.EndTime)));
            this.cacheService = cacheService;
            this.recordTypeId = recordTypeId;
        }

        public string Name
        {
            get { return cacheService.GetItemById<RecordType>(recordTypeId).Name; }
        }

        private string time;

        public string Time
        {
            get { return time; }
            set { Set("Time", ref time, value); }
        }
    }
}