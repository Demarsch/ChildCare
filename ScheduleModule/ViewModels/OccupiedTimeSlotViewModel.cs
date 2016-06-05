using System;
using Core.Data;
using Core.Data.Services;
using Core.Misc;
using Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using ScheduleModule.DTO;
using ScheduleModule.Misc;

namespace ScheduleModule.ViewModels
{
    public class OccupiedTimeSlotViewModel : BindableBase, ITimeInterval
    {
        private ScheduledAssignmentDTO assignment;

        private readonly IEnvironment environment;

        private readonly ISecurityService securityService;

        private readonly ICacheService cacheService;

        public OccupiedTimeSlotViewModel(ScheduledAssignmentDTO assignment, IEnvironment environment, ISecurityService securityService, ICacheService cacheService)
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
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.securityService = securityService;
            this.cacheService = cacheService;
            this.environment = environment;
            this.assignment = assignment;
            CancelOrDeleteCommand = new DelegateCommand(CancelOrDelete, CanCancelOrDelete);
            UpdateCommand = new DelegateCommand(Update, CanUpdateOrMove);
            MoveCommand = new DelegateCommand(Move, CanUpdateOrMove);
            MarkAsCompletedCommand = new DelegateCommand(MarkAsCompleted, CanMarkAsCompleted)
                .ObservesProperty(() => IsCompleted)
                .ObservesProperty(() => IsTemporary);
        }

        public DelegateCommand MarkAsCompletedCommand { get; private set; }

        private void MarkAsCompleted()
        {
            OnCompleteRequested();
        }

        private bool CanMarkAsCompleted()
        {
            if (IsCompleted)
            {
                return false;
            }
            if (IsTemporary)
            {
                return false;
            }
            return securityService.HasPermission(Permission.CompleteAssignments);
        }

        public DelegateCommand CancelOrDeleteCommand { get; private set; }

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
                return assignment.AssignUserId == environment.CurrentUser.Id || securityService.HasPermission(Permission.DeleteTemporaryAssignments);
            }
            return securityService.HasPermission(Permission.EditAssignments);
        }

        public DelegateCommand UpdateCommand { get; private set; }

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
            return securityService.HasPermission(Permission.EditAssignments);
        }

        public DelegateCommand MoveCommand { get; private set; }

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
            OnPropertyChanged(string.Empty);
        }

        public string PersonShortName { get { return assignment.PersonShortName; } }

        public bool IsCompleted
        {
            get { return assignment.IsCompleted; }
            set
            {
                if (assignment.IsCompleted != value)
                {
                    assignment.IsCompleted = value;
                    OnPropertyChanged();
                }
            }
        }

        public int RoomId
        {
            get { return assignment.RoomId; }
            set
            {
                if (assignment.RoomId != value)
                {
                    assignment.RoomId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Room Room { get { return cacheService.GetItemById<Room>(assignment.RoomId); } }

        public int RecordTypeId { get { return assignment.RecordTypeId; } }

        public bool TryUpdate(ScheduledAssignmentDTO assignment)
        {
            if (assignment == null || this.assignment.Id != assignment.Id)
            {
                return false;
            }
            this.assignment = assignment;
            OnPropertyChanged(string.Empty);
            CancelOrDeleteCommand.RaiseCanExecuteChanged();
            UpdateCommand.RaiseCanExecuteChanged();
            MoveCommand.RaiseCanExecuteChanged();
            return true;
        }

        public event EventHandler CompleteRequested;

        protected virtual void OnCompleteRequested()
        {
            var handler = CompleteRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
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
                if (SetProperty(ref isBeingMoved, value))
                {
                    OnPropertyChanged(() => State);
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
                    OnPropertyChanged();
                    OnPropertyChanged(() => State);
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        private bool isInReadOnlyMode;

        public bool IsInReadOnlyMode
        {
            get { return isInReadOnlyMode; }
            set { SetProperty(ref isInReadOnlyMode, value); }
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