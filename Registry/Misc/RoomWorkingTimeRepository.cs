using System;
using System.Collections.Generic;
using System.Linq;

namespace Registry
{
    public class RoomWorkingTimeRepository
    {
        public static readonly TimeSpan DefaultOpenTime = TimeSpan.FromHours(8.0);

        public static readonly TimeSpan DefaultCloseTime = TimeSpan.FromHours(17.0);

        private readonly Dictionary<int, ILookup<int, WorkingTime>> items; 

        public RoomWorkingTimeRepository(IEnumerable<WorkingTime> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            this.items = items.ToLookup(x => x.RoomId).ToDictionary(x => x.Key, x => x.ToLookup(y => y.RecordTypeId));
        }

        public ILookup<int, WorkingTime> GetRoomWorkingTime(int roomId)
        {
            ILookup<int, WorkingTime> result;
            return items.TryGetValue(roomId, out result) ? result : new WorkingTime[0].ToLookup(x => x.RoomId);
        }

        public IEnumerable<WorkingTime> GetRoomAndRecordTypeWorkingTime(int roomId, int recordTypeId)
        {
            return GetRoomWorkingTime(roomId)[recordTypeId];
        }

        public TimeSpan GetOpenTime()
        {
            if (items.Count == 0)
                return DefaultOpenTime;
            return items.Min(x => x.Value.Min(y => y.Min(z => z.StartTime)));
        }

        public TimeSpan GetCloseTime()
        {
            if (items.Count == 0)
                return DefaultCloseTime;
            return items.Max(x => x.Value.Max(y => y.Max(z => z.EndTime)));
        }
    }
}
