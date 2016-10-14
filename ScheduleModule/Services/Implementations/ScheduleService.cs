using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;
using Core.Notification;
using Core.Services;
using ScheduleModule.DTO;
using ScheduleModule.Exceptions;
using Shared.Schedule.Services;
using Core.Extensions;


namespace ScheduleModule.Services
{
    public class ScheduleService : ScheduleServiceBase, IScheduleService
    {
        private readonly IDbContextProvider contextProvider;

        private readonly IEnvironment environment;

        public ScheduleService(IDbContextProvider contextProvider, IEnvironment environment, ICacheService cacheService)
            : base(contextProvider, cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.environment = environment;
            this.contextProvider = contextProvider;
        }

        public ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date)
        {
            var endDate = date.Date.AddDays(1.0);
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                return dataContext.NoTrackingSet<Assignment>()
                                  .Where(x => x.AssignDateTime >= date.Date && x.AssignDateTime < endDate && !x.CancelUserId.HasValue)
                                  .Select(x => new ScheduledAssignmentDTO
                                               {
                                                   Id = x.Id,
                                                   StartTime = x.AssignDateTime,
                                                   Duration = x.Duration,
                                                   IsCompleted = x.IsCompleted || (x.RecordId.HasValue && x.Record.IsCompleted),
                                                   RecordTypeId = x.RecordTypeId,
                                                   RoomId = x.RoomId,
                                                   PersonShortName = x.Person.ShortName,
                                                   IsTemporary = x.IsTemporary,
                                                   AssignUserId = x.AssignUserId,
                                                   AssignLpuId = x.AssignLpuId,
                                                   FinancingSourceId = x.FinancingSourceId,
                                                   FinancingSourceName = x.FinancingSource.ShortName,
                                                   Note = x.Note
                                               })
                                  .ToLookup(x => x.RoomId);
            }
        }

        public IEnumerable<ITimeInterval> GenerateAvailableTimeIntervals(IEnumerable<ITimeInterval> workingTime,
                                                                         IEnumerable<ITimeInterval> occupiedTime,
                                                                         int nominalDurationInMinutes,
                                                                         int minimumDurationInMinutes)
        {
            if (minimumDurationInMinutes <= 0 || minimumDurationInMinutes % 5 != 0)
            {
                throw new ArgumentException("Minimum duration must be greater than zero and be divisable by 5", "minimumDurationInMinutes");
            }
            if (nominalDurationInMinutes <= 0 || nominalDurationInMinutes % 5 != 0)
            {
                throw new ArgumentException("Nominal duration must be greater than zero and be divisable by 5", "nominalDurationInMinutes");
            }
            if (minimumDurationInMinutes > nominalDurationInMinutes)
            {
                throw new ArgumentException("Minimum duration must not be greater than nominal duration", "minimumDurationInMinutes");
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
            {
                times.AddInterval(interval);
            }
            foreach (var interval in occupiedTime)
            {
                times.RemoveInterval(interval);
            }
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
                                                                                                                   freeTime.StartTime.Add(TimeSpan.FromMinutes(nominalDurationInMinutes * (x + 1)))))
                                              .ToList();
                    //Now we look at how much time remains
                    var remainingDuration = totalFreeMinutes - nominalDurationInMinutes * nominalDurationIntervalCount;
                    //If time remaining is greater than minimum duration we just fill this gap
                    if (remainingDuration >= minimumDurationInMinutes)
                    {
                        intervals.Add(new TimeInterval(intervals[intervals.Count - 1].EndTime, freeTime.EndTime));
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

        public async Task SaveAssignmentAsync(Assignment newAssignment, INotificationServiceSubscription<Assignment> assignmentChangeSubscription)
        {
            newAssignment.Note = newAssignment.Note ?? string.Empty;
            Assignment originalAssignment = null;
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                dataContext.Configuration.ProxyCreationEnabled = false;
                if (newAssignment.Id == 0)
                {
                    dataContext.Entry(newAssignment).State = EntityState.Added;
                    newAssignment.CreationDateTime = environment.CurrentDate;
                }
                else
                {
                    originalAssignment = await dataContext.NoTrackingSet<Assignment>().FirstAsync(x => x.Id == newAssignment.Id);
                    dataContext.Entry(newAssignment).State = EntityState.Modified;
                }
                await CheckForAssignmentConflicts(dataContext, newAssignment);
                await dataContext.SaveChangesAsync();
                if (assignmentChangeSubscription != null)
                {
                    assignmentChangeSubscription.Notify(originalAssignment, newAssignment);
                }
            }
        }

        internal async Task CheckForAssignmentConflicts(DbContext dataContext, Assignment newAssignment)
        {
            var startTime = newAssignment.AssignDateTime;
            var endTime = newAssignment.AssignDateTime.AddMinutes(newAssignment.Duration);
            var conflictedAssignment = await dataContext.NoTrackingSet<Assignment>()
                                                        .Where(x => !x.CancelUserId.HasValue && x.Id != newAssignment.Id && (x.RoomId == newAssignment.RoomId || x.PersonId == newAssignment.PersonId))
                                                        .Select(x => new
                                                                     {
                                                                         Room = x.Room.Name + " №" + x.Room.Number,
                                                                         Patient = x.Person.ShortName,
                                                                         IsSamePatient = x.PersonId == newAssignment.PersonId,
                                                                         StartTime = x.AssignDateTime,
                                                                         EndTime = DbFunctions.AddMinutes(x.AssignDateTime, x.Duration)
                                                                     })
                                                        .FirstOrDefaultAsync(x => (x.StartTime >= startTime && x.StartTime < endTime)
                                                                                  || (x.EndTime > startTime && x.EndTime <= endTime)
                                                                                  || (startTime >= x.StartTime && startTime < x.EndTime)
                                                                                  || (endTime > x.StartTime && endTime < x.EndTime));
            if (conflictedAssignment == null)
            {
                return;
            }
            if (conflictedAssignment.IsSamePatient)
            {
                throw new SamePatientAssignmentConflictException(conflictedAssignment.Room, conflictedAssignment.StartTime, conflictedAssignment.Patient);
            }
            throw new AssignmentConflictException(conflictedAssignment.Room, conflictedAssignment.StartTime, conflictedAssignment.Patient);
        }

        public async Task DeleteAssignmentAsync(int assignmentId, INotificationServiceSubscription<Assignment> assignmentChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                dataContext.Configuration.ProxyCreationEnabled = false;
                var originalAssignment = await dataContext.NoTrackingSet<Assignment>().FirstAsync(x => x.Id == assignmentId);
                dataContext.Entry(originalAssignment).State = EntityState.Deleted;
                await dataContext.SaveChangesAsync();
                if (assignmentChangeSubscription != null)
                {
                    assignmentChangeSubscription.NotifyDelete(originalAssignment);
                }
            }
        }

        public async Task CancelAssignmentAsync(int assignmentId, INotificationServiceSubscription<Assignment> assignmentChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                var originalAssignment = await dataContext.NoTrackingSet<Assignment>().FirstAsync(x => x.Id == assignmentId);
                var newAssignment = (Assignment)originalAssignment.Clone();
                newAssignment.CancelUserId = environment.CurrentUser.Id;
                newAssignment.CancelDateTime = environment.CurrentDate;
                dataContext.Entry(newAssignment).State = EntityState.Modified;
                await dataContext.SaveChangesAsync();
                if (assignmentChangeSubscription != null)
                {
                    assignmentChangeSubscription.Notify(originalAssignment, newAssignment);
                }
            }
        }

        public async Task UpdateAssignmentAsync(int assignmentId,
                                                int newFinancingSourceId,
                                                string newNote,
                                                int? newAssignLpuId,
                                                INotificationServiceSubscription<Assignment> assignmentChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                var originalAssignment = await dataContext.NoTrackingSet<Assignment>().FirstAsync(x => x.Id == assignmentId);
                var newAssignment = (Assignment)originalAssignment.Clone();
                newAssignment.FinancingSourceId = newFinancingSourceId;
                newAssignment.Note = newNote;
                newAssignment.AssignLpuId = newAssignLpuId;
                newAssignment.IsTemporary = false;
                dataContext.Entry(newAssignment).State = EntityState.Modified;
                await dataContext.SaveChangesAsync();
                if (assignmentChangeSubscription != null)
                {
                    assignmentChangeSubscription.Notify(originalAssignment, newAssignment);
                }
            }
        }

        public async Task MarkAssignmentCompletedAsync(int assignmentId, DateTime completeDataTime, INotificationServiceSubscription<Assignment> assignmentChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                var originalAssignment = await dataContext.NoTrackingSet<Assignment>().FirstAsync(x => x.Id == assignmentId);
                var newAssignment = (Assignment)originalAssignment.Clone();
                newAssignment.IsCompleted = true;
                dataContext.Entry(newAssignment).State = EntityState.Modified;
                await dataContext.SaveChangesAsync();
                if (assignmentChangeSubscription != null)
                {
                    assignmentChangeSubscription.Notify(originalAssignment, newAssignment);
                }
            }
        }

        public async Task MoveAssignmentAsync(int assignmentId, DateTime newTime, int newDuration, Room newRoom, INotificationServiceSubscription<Assignment> assignmentChangeSubscription)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                var originalAssignment = await dataContext.NoTrackingSet<Assignment>().FirstAsync(x => x.Id == assignmentId);
                var newAssignment = (Assignment)originalAssignment.Clone();
                newAssignment.AssignDateTime = newTime;
                newAssignment.Duration = newDuration;
                newAssignment.RoomId = newRoom.Id;
                await CheckForAssignmentConflicts(dataContext, newAssignment);
                dataContext.Entry(newAssignment).State = EntityState.Modified;
                await dataContext.SaveChangesAsync();
                if (assignmentChangeSubscription != null)
                {
                    assignmentChangeSubscription.Notify(originalAssignment, newAssignment);
                }
            }
        }

        public IEnumerable<ScheduledAssignmentDTO> GetAssignments(int patientId)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                return dataContext.Set<Assignment>().Where(x => x.PersonId == patientId)
                                  .Select(x => new ScheduledAssignmentDTO
                                               {
                                                   Id = x.Id,
                                                   StartTime = x.AssignDateTime,
                                                   IsCompleted = x.IsCompleted || (x.RecordId.HasValue && x.Record.IsCompleted),
                                                   IsTemporary = x.IsTemporary,
                                                   RecordTypeId = x.RecordTypeId,
                                                   RoomId = x.RoomId,
                                                   Duration = x.Duration,
                                                   AssignLpuId = x.AssignLpuId,
                                                   AssignUserId = x.AssignUserId,
                                                   FinancingSourceId = x.FinancingSourceId,
                                                   FinancingSourceName = x.FinancingSource.ShortName,
                                                   Note = x.Note,
                                                   PersonShortName = x.Person.ShortName
                                               })
                                  .ToArray();
            }
        }

        public IEnumerable<ScheduledAssignmentDTO> GetActualAssignments(int patientId, DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1.0);
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                return dataContext.Set<Assignment>().Where(x => x.PersonId == patientId && x.CancelUserId == null && x.AssignDateTime >= startDate && x.AssignDateTime < endDate)
                                  .Select(x => new ScheduledAssignmentDTO
                                               {
                                                   Id = x.Id,
                                                   StartTime = x.AssignDateTime,
                                                   IsCompleted = x.IsCompleted || (x.RecordId.HasValue && x.Record.IsCompleted),
                                                   IsTemporary = x.IsTemporary,
                                                   RecordTypeId = x.RecordTypeId,
                                                   RoomId = x.RoomId,
                                                   Duration = x.Duration,
                                                   AssignLpuId = x.AssignLpuId,
                                                   AssignUserId = x.AssignUserId,
                                                   FinancingSourceId = x.FinancingSourceId,
                                                   FinancingSourceName = x.FinancingSource.ShortName,
                                                   Note = x.Note,
                                                   PersonShortName = x.Person.ShortName
                                               })
                                  .ToArray();
            }
        }

        public async Task<ScheduledAssignmentDTO> GetAssignmentAsync(int assignmentId, int patientId)
        {
            using (var dataContext = contextProvider.CreateLightweightContext())
            {
                var query = dataContext.Set<Assignment>() as IQueryable<Assignment>;
                query = patientId.IsNewOrNonExisting()
                            ? query.Where(x => x.Id == assignmentId)
                            : query.Where(x => x.Id == assignmentId && x.PersonId == patientId);
                return await query.Select(x => new ScheduledAssignmentDTO
                                               {
                                                   Id = x.Id,
                                                   StartTime = x.AssignDateTime,
                                                   IsCompleted = x.IsCompleted || (x.RecordId.HasValue && x.Record.IsCompleted),
                                                   IsTemporary = x.IsTemporary,
                                                   RecordTypeId = x.RecordTypeId,
                                                   RoomId = x.RoomId,
                                                   Duration = x.Duration,
                                                   AssignLpuId = x.AssignLpuId,
                                                   AssignUserId = x.AssignUserId,
                                                   FinancingSourceId = x.FinancingSourceId,
                                                   FinancingSourceName = x.FinancingSource.ShortName,
                                                   Note = x.Note,
                                                   PersonShortName = x.Person.ShortName,
                                                   IsCanceled = x.CancelUserId.HasValue
                                               })
                                  .FirstOrDefaultAsync();
            }
        }

        public IDisposableQueryable<Person> GetPatientQuery(int currentPatient)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().Where(x => x.Id == currentPatient), context);
        }

        public IEnumerable<Org> GetLpus()
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                return context.NoTrackingSet<Org>()
                              .Where(x => x.IsLpu)
                              .OrderBy(x => x.Name)
                              .ToArray();
            }
        }


        public async Task<IEnumerable<KeyValuePair<int, ITimeInterval>>> GetAvailiableTimeSlots(DateTime date, RecordType selectedRecordType, Room selectedRoom = null, bool checkExistingAssignments = true)
        {
            var timeSlots = new List<KeyValuePair<int, ITimeInterval>>();

            using (var context = contextProvider.CreateNewContext())
            {
                var rooms = new List<Room>();
                var workingTimes = (await Task<IEnumerable<ScheduleItem>>.Factory.StartNew(x => this.GetRoomsWorkingTimeForDay((DateTime)x), date))
                    .Where(x => x.RecordTypeId.HasValue && x.RecordTypeId == selectedRecordType.Id && (selectedRoom == null || selectedRoom.Id == x.RoomId))
                    .Select(x => new { x.RoomId, Times = new TimeInterval(x.StartTime, x.EndTime) })
                    .ToArray();
                foreach (var workingTime in workingTimes.GroupBy(x => x.RoomId))
                {
                    var assignments = checkExistingAssignments ? await Task<ILookup<int, ScheduledAssignmentDTO>>.Factory.StartNew(x => this.GetRoomsAssignments(date), date) : null;
                    var availableTimeIntervals = this.GenerateAvailableTimeIntervals
                        (
                        workingTime.Select(x => x.Times),
                        assignments.SelectMany(x => x),
                        selectedRecordType.Duration,
                        selectedRecordType.MinDuration
                        );
                    timeSlots = timeSlots.Union(availableTimeIntervals.Select(x => new KeyValuePair<int, ITimeInterval>(workingTime.Key, x))).ToList();
                }
            }
            return timeSlots;
        }
    }
}