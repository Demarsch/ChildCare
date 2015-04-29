using System;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
    public class TimeIntervalCollection : IEnumerable<ITimeInterval>
    {
        private readonly List<TimeInterval> intervals = new List<TimeInterval>();

        public void AddInterval(ITimeInterval addedInterval)
        {
            if (intervals.Count == 0)
            {
                intervals.Add(new TimeInterval(addedInterval.StartTime, addedInterval.EndTime));
                return;
            }
            for (var index = 0; index < intervals.Count; index++)
            {
                var currentInterval = intervals[index];
                var relation = GetTimeIntervalRelation(addedInterval, currentInterval);
                switch (relation)
                {
                    case TimeIntervalRelation.Before:
                        intervals.Insert(0, new TimeInterval(addedInterval.StartTime, addedInterval.EndTime));
                        goto EndOfLoop;
                    case TimeIntervalRelation.After:
                        if (index == intervals.Count - 1)
                            intervals.Add(new TimeInterval(addedInterval.StartTime, addedInterval.EndTime));
                        break;
                    case TimeIntervalRelation.StartTouching:
                    case TimeIntervalRelation.StartInside:
                        addedInterval = new TimeInterval(currentInterval.StartTime, addedInterval.EndTime);
                        if (index == intervals.Count - 1)
                            intervals[index] = addedInterval as TimeInterval;
                        else
                        {
                            intervals.RemoveAt(index);
                            index--;
                        }
                        break;
                    case TimeIntervalRelation.EndTouching:
                    case TimeIntervalRelation.EndInside:
                        addedInterval = new TimeInterval(addedInterval.StartTime, currentInterval.EndTime);
                        intervals[index] = addedInterval as TimeInterval;
                        goto EndOfLoop;
                    case TimeIntervalRelation.Contains:
                        addedInterval = new TimeInterval(addedInterval.StartTime, addedInterval.EndTime);
                        if (index == intervals.Count - 1)
                            intervals[index] = addedInterval as TimeInterval;
                        else
                        {
                            intervals.RemoveAt(index);
                            index--;
                        }
                        break;
                    case TimeIntervalRelation.IsContained:
                    case TimeIntervalRelation.Same:
                        goto EndOfLoop;
                }
            }
    EndOfLoop:
            ;
        }

        public void RemoveInterval(ITimeInterval subtractedInterval)
        {
            if (intervals.Count == 0)
            {
                return;
            }
            for (int index = 0; index < intervals.Count; index++)
            {
                var currentInterval = intervals[index];
                var relation = GetTimeIntervalRelation(subtractedInterval, currentInterval);
                switch (relation)
                {
                    case TimeIntervalRelation.Before:
                    case TimeIntervalRelation.EndTouching:
                        goto EndOfLoop;
                    case TimeIntervalRelation.After:
                    case TimeIntervalRelation.StartTouching:
                        continue;
                    case TimeIntervalRelation.StartInside:
                        var tempInterval = new TimeInterval(currentInterval.EndTime, subtractedInterval.EndTime);
                        currentInterval.EndTime = subtractedInterval.StartTime;
                        subtractedInterval = tempInterval;
                        break;
                    case TimeIntervalRelation.EndInside:
                        currentInterval.StartTime = subtractedInterval.EndTime;
                        goto EndOfLoop;
                    case TimeIntervalRelation.Contains:
                        subtractedInterval = new TimeInterval(currentInterval.EndTime, subtractedInterval.EndTime);
                        intervals.RemoveAt(index);
                        index--;
                        break;
                    case TimeIntervalRelation.IsContained:
                        if (subtractedInterval.StartTime == currentInterval.StartTime)
                            currentInterval.StartTime = subtractedInterval.EndTime;
                        else if (subtractedInterval.EndTime == currentInterval.EndTime)
                            currentInterval.EndTime = subtractedInterval.StartTime;
                        else
                        {
                            var leftInterval = new TimeInterval(currentInterval.StartTime, subtractedInterval.StartTime);
                            var rightInterval = new TimeInterval(subtractedInterval.EndTime, currentInterval.EndTime);
                            intervals.RemoveAt(index);
                            intervals.Insert(index, rightInterval);
                            intervals.Insert(index, leftInterval);
                        }
                        goto EndOfLoop;
                    case TimeIntervalRelation.Same:
                        intervals.RemoveAt(index);
                        goto EndOfLoop;
                }
            }
        EndOfLoop:
            ;
        }


        public IEnumerator<ITimeInterval> GetEnumerator()
        {
            return intervals.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static TimeIntervalRelation GetTimeIntervalRelation(ITimeInterval first, ITimeInterval second)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");

            if (first.EndTime < second.StartTime)
                return TimeIntervalRelation.Before;
            if (first.StartTime > second.EndTime)
                return TimeIntervalRelation.After;

            if (first.EndTime == second.StartTime)
                return TimeIntervalRelation.EndTouching;
            if (first.StartTime == second.EndTime)
                return TimeIntervalRelation.StartTouching;

            if (first.StartTime == second.StartTime && first.EndTime == second.EndTime)
                return TimeIntervalRelation.Same;
            if (first.StartTime >= second.StartTime && first.EndTime <= second.EndTime)
                return TimeIntervalRelation.IsContained;
            if (first.StartTime <= second.StartTime && first.EndTime >= second.EndTime)
                return TimeIntervalRelation.Contains;

            if (first.StartTime > second.StartTime && first.EndTime > second.EndTime)
                return TimeIntervalRelation.StartInside;
            if (first.StartTime < second.StartTime && first.EndTime < second.EndTime)
                return TimeIntervalRelation.EndInside;
            throw new ArgumentException("Can't identify the relationship between intervals");
        }
    }

    internal enum TimeIntervalRelation
    {
        Before,
        After,
        StartInside,
        EndInside,
        StartTouching,
        EndTouching,
        Contains,
        IsContained,
        Same
    }
}
