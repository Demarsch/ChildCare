using System;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class ScheduleCellViewModel : ObservableObject
    {
        public ScheduleCellViewModel(DateTime startTime, DateTime endTime)
        {
            if (startTime >= endTime)
            {
                throw new ArgumentException("Start time must be less than end time");
            }
            StartTime = startTime;
            EndTime = endTime;
        }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }
    }
}
