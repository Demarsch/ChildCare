using System;
using Core;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class ScheduledAssignmentViewModel : ObservableObject, ITimeInterval
    {
        private ScheduledAssignmentDTO assignment;

        public ScheduledAssignmentViewModel(ScheduledAssignmentDTO assignment)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            this.assignment = assignment;
        }

        public int Id { get { return assignment.Id; } }

        public DateTime StartTime { get { return assignment.StartTime; } }

        public bool IsTemporary { get { return assignment.IsTemporary; } }

        public DateTime EndTime { get { return assignment.EndTime; } }

        public string PersonShortName { get { return assignment.PersonShortName; } }

        public bool IsCompleted { get { return assignment.IsCompleted; } }

        public bool TryUpdate(ScheduledAssignmentDTO assignment)
        {
            if (assignment == null || this.assignment.Id != assignment.Id)
            {
                return false;
            }
            this.assignment = assignment;
            RaisePropertyChanged(string.Empty);
            return true;
        }

        TimeSpan ITimeInterval.StartTime
        {
            get { return StartTime.TimeOfDay; }
        }

        TimeSpan ITimeInterval.EndTime
        {
            get { return EndTime.TimeOfDay; }
        }

        public ScheduledAssignmentViewModelState State
        {
            get
            {
                if (IsTemporary)
                {
                    return ScheduledAssignmentViewModelState.Temporary;
                }
                if (IsCompleted)
                {
                    return ScheduledAssignmentViewModelState.Completed;
                }
                return ScheduledAssignmentViewModelState.Uncompleted;
            }
        }
    }

    public enum ScheduledAssignmentViewModelState
    {
        Temporary,
        Uncompleted,
        Completed,
    }
}