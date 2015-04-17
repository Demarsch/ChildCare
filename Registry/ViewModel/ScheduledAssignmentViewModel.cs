using System;
using Core;
using DataLib;

namespace Registry
{
    public class ScheduledAssignmentViewModel
    {
        private readonly ScheduledAssignmentDTO assignment;

        public ScheduledAssignmentViewModel(ScheduledAssignmentDTO assignment, ICacheService cacheService)
        {
            if (assignment == null)
                throw new ArgumentNullException("assignment");
            if (cacheService == null)
                throw new ArgumentNullException("cacheService");
            this.assignment = assignment;
            StartTime = assignment.AssignDateTime.TimeOfDay;
            EndTime = assignment.AssignDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(cacheService.GetItemById<RecordType>(assignment.RecordTypeId).Duration));
        }

        public TimeSpan StartTime { get; private set; }

        public TimeSpan EndTime { get; private set; }

        public string PersonShortName { get { return assignment.PersonShortName; } }
    }
}