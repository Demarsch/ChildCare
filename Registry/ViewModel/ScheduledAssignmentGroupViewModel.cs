using System;

namespace Registry
{
    public class ScheduledAssignmentGroupViewModel
    {
        public ScheduledAssignmentGroupViewModel(TimeSpan startTime, TimeSpan endTime, string patientSummary)
        {
            StartTime = startTime;
            EndTime = endTime;
            PatientSummary = patientSummary;
        }

        public TimeSpan StartTime { get; private set; }

        public TimeSpan EndTime { get; private set; }

        public string PatientSummary { get; private set; }
    }
}