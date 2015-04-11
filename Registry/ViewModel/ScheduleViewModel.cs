using System;
using System.Collections.Generic;
using Core;

namespace Registry
{
    public class ScheduleViewModel : BasicViewModel
    {
        public ScheduleViewModel()
        {
            selectedDate = DateTime.Today;
            //TODO: remove this fake data
            OpenTime = new TimeSpan(8, 0, 0);
            CloseTime = new TimeSpan(17, 0, 0);
            TimeTickerStep = TimeSpan.FromMinutes(30.0);
        }

        private DateTime selectedDate;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set { Set("SelectedDate", ref selectedDate, value); }
        }

        #region Time tickers

        private IEnumerable<TimeSpan> timeTickers;

        public IEnumerable<TimeSpan> TimeTickers
        {
            get { return timeTickers; }
            set { Set("TimeTickers", ref timeTickers, value); }
        }

        private TimeSpan openTime;

        public TimeSpan OpenTime
        {
            get { return openTime; }
            set
            {
                if (Set("OpenTime", ref openTime, value))
                    UpdateTimeTickers();
            }
        }

        private TimeSpan closeTime;

        public TimeSpan CloseTime
        {
            get { return closeTime; }
            set
            {
                if (Set("CloseTime", ref closeTime, value))
                    UpdateTimeTickers();
            }
        }

        private TimeSpan timeTickerStep;

        public TimeSpan TimeTickerStep
        {
            get { return timeTickerStep; }
            set
            {
                if (Set("TimeTickerStep", ref timeTickerStep, value))
                    UpdateTimeTickers();
            }
        }

        private void UpdateTimeTickers()
        {
            if (openTime < TimeSpan.Zero
                || closeTime < TimeSpan.Zero
                || openTime >= closeTime
                || timeTickerStep <= TimeSpan.Zero)
            {
                TimeTickers = new TimeSpan[0];
                return;
            }
            var result = new List<TimeSpan>();
            var currentTime = openTime;
            while (currentTime < closeTime)
            {
                result.Add(currentTime);
                currentTime = currentTime.Add(timeTickerStep);
            }
            TimeTickers = result;
        }

        #endregion
    }
}
