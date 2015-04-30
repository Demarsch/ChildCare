using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Registry
{
    public class ScheduleService : IScheduleService
    {
        private readonly IDataContextProvider dataContextProvider;

        public ScheduleService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            this.dataContextProvider = dataContextProvider;
        }

        public ICollection<Room> GetRooms()
        {
            return dataContextProvider.StaticDataContext.GetData<Room>().ToArray();
        }

        public ICollection<RecordType> GetRecordTypes()
        {
            return dataContextProvider.StaticDataContext.GetData<RecordType>().ToArray();
        }

        public ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date)
        {
            var endDate = date.Date.AddDays(1.0);
            using (var dataContext = dataContextProvider.GetNewDataContext())
                return dataContext.GetData<Assignment>()
                    .Where(x => x.AssignDateTime >= date.Date && x.AssignDateTime < endDate && !x.CancelUserId.HasValue)
                    .OrderBy(x => x.AssignDateTime)
                    .Select(x => new ScheduledAssignmentDTO
                    {
                        Id = x.Id,
                        StartTime = x.AssignDateTime,
                        Duration = x.RecordType.Duration,
                        IsCompleted = x.RecordId.HasValue && x.Record.IsCompleted,
                        RecordTypeId = x.RecordTypeId,
                        RoomId = x.RoomId,
                        PersonShortName = x.Person.ShortName
                    })
                    .ToLookup(x => x.RoomId);
        }

        public ICollection<ScheduleItemDTO> GetRoomsWorkingTime(DateTime date)
        {
            date = date.Date;
            var dayOfWeek = (int)date.DayOfWeek;
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                var result = dataContext.GetData<ScheduleItem>().Where(x => x.BeginDate <= date && x.EndDate >= date && x.DayOfWeek == dayOfWeek);
                return result.GroupBy(x => new { x.RoomId, x.RecordTypeId })
                    .SelectMany(x => x.GroupBy(y => new { y.BeginDate, y.EndDate }).OrderByDescending(y => y.Key.BeginDate).ThenBy(y => y.Key.EndDate).FirstOrDefault())
                    .Select(x => new ScheduleItemDTO
                    {
                        RoomId = x.RoomId, 
                        RecordTypeId = x.RecordTypeId,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime
                    })
                    .ToArray();
            }
        }

        public IEnumerable<ITimeInterval> GetAvailableTimeIntervals(IEnumerable<ITimeInterval> workingTime, IEnumerable<ITimeInterval> occupiedTime, int nominalDurationInMinutes, int minimumDurationInMinutes)
        {
            if (minimumDurationInMinutes <= 0 || minimumDurationInMinutes % 5 != 0)
            {
                throw new ArgumentException("Minimum duration must be grater than zero and be divisable by 5", "minimumDurationInMinutes");
            }
            if (nominalDurationInMinutes <= 0 || nominalDurationInMinutes % 5 != 0)
            {
                throw new ArgumentException("Nominal duration must be grater than zero and be divisable by 5", "nominalDurationInMinutes");
            }
            if (minimumDurationInMinutes > nominalDurationInMinutes)
            {
                throw new ArgumentException("Minimum duration must not be grater than nominal duration", "minimumDurationInMinutes");
            }
            if (workingTime == null)
            {
                workingTime = new ITimeInterval[0];
            }
            if (occupiedTime == null)
            {
                occupiedTime = new ITimeInterval[0];
            }
            var times = new TimeIntervalCollection();
            foreach (var interval in workingTime)
                times.AddInterval(interval);
            foreach (var interval in occupiedTime)
                times.RemoveInterval(interval);
            var result = new List<ITimeInterval>();
            var fiveMinutes = TimeSpan.FromMinutes(5.0);
            foreach (var freeTime in times)
            {
                var totalFreeMinutes = (int)freeTime.EndTime.Subtract(freeTime.StartTime).TotalMinutes;
                //If total time consists of the whole number of nominal duration intervals then we just split in into pieces
                if (totalFreeMinutes % nominalDurationInMinutes == 0)
                {
                    var intervalCount = totalFreeMinutes / nominalDurationInMinutes;
                    result.AddRange(Enumerable.Range(0, intervalCount)
                        .Select(x => new TimeInterval(freeTime.StartTime.Add(TimeSpan.FromMinutes(nominalDurationInMinutes * x)), 
                                                      freeTime.StartTime.Add(TimeSpan.FromMinutes(nominalDurationInMinutes * (x + 1))))));
                }
                //If total time is less then minimum duration we can't put assignment inside it
                else if (totalFreeMinutes < minimumDurationInMinutes)
                {
                    continue;
                }
                //If total time is between minimum and nominal duration we just use it all
                else if (totalFreeMinutes < nominalDurationInMinutes)
                {
                    result.Add(new TimeInterval(freeTime.StartTime, freeTime.EndTime));
                }
                //If we can't split total time in whole pieces...
                else
                {
                    var nominalDurationIntervalCount = totalFreeMinutes / nominalDurationInMinutes;
                    //...we first create maximum number of nominal duration intervals
                    var intervals = Enumerable.Range(0, nominalDurationIntervalCount).Select(x => new TimeInterval(freeTime.StartTime.Add(TimeSpan.FromMinutes(nominalDurationInMinutes * x)),
                        freeTime.StartTime.Add(TimeSpan.FromMinutes(nominalDurationInMinutes * (x + 1))))).ToList();
                    //Now we look at how much time remains
                    var remainingDuration = totalFreeMinutes - nominalDurationInMinutes * nominalDurationIntervalCount;
                    //If time remaining is greate than minimum duration we just fill this gap
                    if (remainingDuration >= minimumDurationInMinutes)
                    {
                        intervals.Add(new TimeInterval(intervals[intervals.Count - 1].EndTime,  freeTime.EndTime));
                        result.AddRange(intervals);
                        continue;
                    }
                    //Otherwise we start to subtract 5 minutes from nominal duration intervals and add it to remaining time window while untill we can finally close the gap or there is no more intervals to decrease
                    var intervalIndex = intervals.Count - 1;
                    var currentDuration = remainingDuration;
                    var intervalWasDecreased = false;
                    do
                    {
                        //If we decreased all intervals by 5 minutes and we still don't have enough free time we try it again
                        if (intervalIndex == -1)
                        {
                            //But if all intervals already have minimal duration then we can't get any more free time
                            if (!intervalWasDecreased)
                            {
                                break;
                            }
                            intervalIndex = intervals.Count - 1;
                            intervalWasDecreased = false;
                        }
                        var interval = intervals[intervalIndex];
                        //If we can decrease interval...
                        if ((int)interval.EndTime.Subtract(interval.StartTime).TotalMinutes > minimumDurationInMinutes)
                        {
                            //...then we decrease it and move all further intervals back
                            interval.EndTime = interval.EndTime.Subtract(fiveMinutes);
                            currentDuration += 5;
                            for (var decreasingIntervalIndex = intervalIndex + 1; decreasingIntervalIndex < intervals.Count; decreasingIntervalIndex++)
                            {
                                var decreasingInterval = intervals[decreasingIntervalIndex];
                                decreasingInterval.StartTime = decreasingInterval.StartTime.Subtract(fiveMinutes);
                                decreasingInterval.EndTime = decreasingInterval.EndTime.Subtract(fiveMinutes);
                            }
                            intervalWasDecreased = true;
                        }
                        intervalIndex--;
                    } while (currentDuration < minimumDurationInMinutes);
                    if (currentDuration >= minimumDurationInMinutes)
                    {
                        intervals.Add(new TimeInterval(intervals[intervals.Count - 1].EndTime, freeTime.EndTime));
                    }
                    result.AddRange(intervals);
                }
            }
            return result;
        }
    }
}