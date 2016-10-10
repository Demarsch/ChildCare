using Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleModule.Misc
{
    public class SelectedTimeIntervalWrapper
    {
        private Dictionary<DateTime, IList<ITimeInterval>> SelectedTimeIntervals { get; set; }

        public SelectedTimeIntervalWrapper()
        {
            SelectedTimeIntervals = new Dictionary<DateTime, IList<ITimeInterval>>();
        }


        public bool IsExistTimeInterval(ITimeInterval timeInterval, DateTime date)
        {
            return SelectedTimeIntervals[date.Date].Any(x => (x.EndTime <= timeInterval.StartTime || x.StartTime >= timeInterval.EndTime));
        }

        public void AddTimeInterval(ITimeInterval timeInterval, DateTime date)
        {
            if (SelectedTimeIntervals.ContainsKey(date.Date))
                SelectedTimeIntervals[date.Date].Add(timeInterval);
            else
                SelectedTimeIntervals.Add(date.Date, new List<ITimeInterval>() { timeInterval });
        }

        public void RemoveTimeInterval(ITimeInterval timeInterval, DateTime date)
        {
            if (!SelectedTimeIntervals.ContainsKey(date.Date))
                return;
            SelectedTimeIntervals[date.Date].Remove(timeInterval);
            if (SelectedTimeIntervals[date.Date].Count < 1)
                SelectedTimeIntervals.Remove(date.Date);
        }
    }
}
