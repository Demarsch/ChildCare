using System;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class OccupiedTimeSlotViewModel : ObservableObject, ITimeInterval
    {
        private ScheduledAssignmentDTO assignment;

        private readonly IEnvironment environment;

        public OccupiedTimeSlotViewModel(ScheduledAssignmentDTO assignment, IEnvironment environment)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            this.environment = environment;
            this.assignment = assignment;
            CancelOrDeleteCommand = new RelayCommand(CancelOrDelete, CanCancelOrDelete);
        }

        public ICommand CancelOrDeleteCommand { get; private set; }

        private void CancelOrDelete()
        {
            OnCancelOrDeleteRequested();
        }

        private bool CanCancelOrDelete()
        {
            if (IsTemporary)
            {
                return assignment.AssignUserId == environment.CurrentUser.UserId;
            }
            else
            {
                return true;
            }
        }

        public int Id { get { return assignment.Id; } }

        public DateTime StartTime { get { return assignment.StartTime; } }

        public bool IsTemporary { get { return assignment.IsTemporary; } }

        public DateTime EndTime { get { return assignment.EndTime; } }

        public string PersonShortName { get { return assignment.PersonShortName; } }

        public bool IsCompleted { get { return assignment.IsCompleted; } }

        public int RoomId { get { return assignment.RoomId; } }

        public bool TryUpdate(ScheduledAssignmentDTO assignment)
        {
            if (assignment == null || this.assignment.Id != assignment.Id)
            {
                return false;
            }
            this.assignment = assignment;
            RaisePropertyChanged(string.Empty);
            (CancelOrDeleteCommand as RelayCommand).RaiseCanExecuteChanged();
            return true;
        }

        public event EventHandler CancelOrDeleteRequested;

        protected virtual void OnCancelOrDeleteRequested()
        {
            var handler = CancelOrDeleteRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        TimeSpan ITimeInterval.StartTime
        {
            get { return StartTime.TimeOfDay; }
        }

        TimeSpan ITimeInterval.EndTime
        {
            get { return EndTime.TimeOfDay; }
        }

        public OccupiedTimeSlotViewModelState State
        {
            get
            {
                if (IsTemporary)
                {
                    return OccupiedTimeSlotViewModelState.Temporary;
                }
                if (IsCompleted)
                {
                    return OccupiedTimeSlotViewModelState.Completed;
                }
                return OccupiedTimeSlotViewModelState.Uncompleted;
            }
        }

        public string CancelOrDeleteActionName
        {
            get { return IsTemporary ? "Удалить назначение" : "Отменить назначение"; }
        }
    }

    public enum OccupiedTimeSlotViewModelState
    {
        Temporary,
        Uncompleted,
        Completed,
    }
}