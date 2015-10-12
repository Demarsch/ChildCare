using System;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class OccupiedTimeSlotViewModel : ObservableObject, ITimeInterval
    {
        private ScheduledAssignmentDTO assignment;

        private readonly IEnvironment environment;

        private readonly ISecurityService securityService;

        public OccupiedTimeSlotViewModel(ScheduledAssignmentDTO assignment, IEnvironment environment, ISecurityService securityService)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            this.securityService = securityService;
            this.environment = environment;
            this.assignment = assignment;
            CancelOrDeleteCommand = new RelayCommand(CancelOrDelete, CanCancelOrDelete);
            UpdateCommand = new RelayCommand(Update, CanUpdateOrMove);
            MoveCommand = new RelayCommand(Move, CanUpdateOrMove);
        }

        public RelayCommand CancelOrDeleteCommand { get; private set; }

        private void CancelOrDelete()
        {
            OnCancelOrDeleteRequested();
        }

        private bool CanCancelOrDelete()
        {
            if (IsCompleted)
            {
                return false;
            }
            if (IsTemporary)
            {
                return assignment.AssignUserId == environment.CurrentUser.UserId || securityService.HasPrivilege(Privileges.DeleteTemporaryAssignments);
            }
            return securityService.HasPrivilege(Privileges.EditAssignments);
        }

        public RelayCommand UpdateCommand { get; private set; }

        private void Update()
        {
            OnUpdateRequested();
        }

        private bool CanUpdateOrMove()
        {
            if (IsCompleted)
            {
                return false;
            }
            if (IsTemporary)
            {
                return false;
            }
            return securityService.HasPrivilege(Privileges.EditAssignments);
        }

        public RelayCommand MoveCommand { get; private set; }

        private void Move()
        {
            OnMoveRequested();
        }

        public int Id { get { return assignment.Id; } }

        public DateTime StartTime { get { return assignment.StartTime; } }

        public DateTime EndTime { get { return assignment.EndTime; } }

        public void UpdateTime(DateTime startTime, DateTime endTime)
        {
            assignment.StartTime = startTime;
            assignment.Duration = (int)endTime.Subtract(startTime).TotalMinutes;
            RaisePropertyChanged(string.Empty);
        }

        public string PersonShortName { get { return assignment.PersonShortName; } }

        public bool IsCompleted { get { return assignment.IsCompleted; } }

        public int RoomId
        {
            get { return assignment.RoomId; }
            set
            {
                if (assignment.RoomId != value)
                {
                    assignment.RoomId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int RecordTypeId { get { return assignment.RecordTypeId; } }

        public bool TryUpdate(ScheduledAssignmentDTO assignment)
        {
            if (assignment == null || this.assignment.Id != assignment.Id)
            {
                return false;
            }
            this.assignment = assignment;
            RaisePropertyChanged(string.Empty);
            CancelOrDeleteCommand.RaiseCanExecuteChanged();
            UpdateCommand.RaiseCanExecuteChanged();
            MoveCommand.RaiseCanExecuteChanged();
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

        public event EventHandler UpdateRequested;

        protected virtual void OnUpdateRequested()
        {
            var handler = UpdateRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler MoveRequested;

        protected virtual void OnMoveRequested()
        {
            var handler = MoveRequested;
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
                if (IsBeingMoved)
                {
                    return OccupiedTimeSlotViewModelState.IsBeingMoved;
                }
                return OccupiedTimeSlotViewModelState.Uncompleted;
            }
        }

        public string CancelOrDeleteActionName
        {
            get { return IsTemporary ? "Удалить назначение" : "Отменить назначение"; }
        }

        private bool isBeingMoved;

        public bool IsBeingMoved
        {
            get { return isBeingMoved; }
            set
            {
                if (Set("IsBeingMoved", ref isBeingMoved, value))
                {
                    RaisePropertyChanged("State");
                }
            }
        }

        public bool IsTemporary
        {
            get { return assignment.IsTemporary; }
            set
            {
                if (assignment.IsTemporary != value)
                {
                    assignment.IsTemporary = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("State");
                    CancelOrDeleteCommand.RaiseCanExecuteChanged();
                    UpdateCommand.RaiseCanExecuteChanged();
                    MoveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Note
        {
            get { return assignment.Note; }
            set
            {
                if (assignment.Note != value)
                {
                    assignment.Note = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int FinancingSourceId
        {
            get { return assignment.FinancingSourceId; }
            set
            {
                if (assignment.FinancingSourceId != value)
                {
                    assignment.FinancingSourceId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int? AssignLpuId
        {
            get { return assignment.AssignLpuId; }
            set
            {
                if (assignment.AssignLpuId != value)
                {
                    assignment.AssignLpuId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool isInReadOnlyMode;

        public bool IsInReadOnlyMode
        {
            get { return isInReadOnlyMode; }
            set { Set("IsInReadOnlyMode", ref isInReadOnlyMode, value); }
        }
    }

    public enum OccupiedTimeSlotViewModelState
    {
        Temporary,
        Uncompleted,
        Completed,
        IsBeingMoved
    }
}