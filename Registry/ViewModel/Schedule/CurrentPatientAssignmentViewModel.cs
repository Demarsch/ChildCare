using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class CurrentPatientAssignmentViewModel : ObservableObject
    {
        private readonly AssignmentScheduleDTO assignment;

        private readonly ICacheService cacheService;

        public CurrentPatientAssignmentViewModel(AssignmentScheduleDTO assignment, ICacheService cacheService)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            this.assignment = assignment;
        }

        public int Id { get { return assignment.Id; } }

        public DateTime StartTime { get { return assignment.AssignDateTime; } }

        public DateTime EndTime { get { return assignment.AssignDateTime.AddMinutes(assignment.Duration); } }

        public string RecordType { get { return cacheService.GetItemById<RecordType>(assignment.RecordTypeId).Name; } }

        private string room;

        public string Room
        {
            get
            {
                if (room == null)
                {
                    var roomObj = cacheService.GetItemById<Room>(assignment.RoomId);
                    room = string.Format("№{0} {1}", roomObj.Number, roomObj.Name);
                }
                return room;
            }
        }

        public CurrentPatientAssignmentViewModelState State
        {
            get
            {
                return assignment.IsTemporary
                    ? CurrentPatientAssignmentViewModelState.Temporary
                    : assignment.IsCompleted
                        ? CurrentPatientAssignmentViewModelState.Completed
                        : CurrentPatientAssignmentViewModelState.Uncompleted;
            }
        }
    }
    
    public enum CurrentPatientAssignmentViewModelState
    {
        Temporary,
        Uncompleted,
        Completed
    }
}
