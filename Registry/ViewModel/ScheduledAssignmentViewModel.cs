using System;

namespace Registry
{
    public class ScheduledAssignmentViewModel
    {
        private readonly ScheduledAssignmentDTO assignment;

        public ScheduledAssignmentViewModel(ScheduledAssignmentDTO assignment)
        {
            if (assignment == null)
                throw new ArgumentNullException("assignment");
            this.assignment = assignment;
            StartTime = assignment.StartTime;
            EndTime = assignment.EndTime;
        }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public string PersonShortName { get { return assignment.PersonShortName; } }

        public bool IsComplete { get { return assignment.IsCompleted; } }
    }
}