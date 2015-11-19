using System;
using Core.Data;
using Core.Services;
using Prism.Mvvm;
using ScheduleModule.DTO;

namespace ScheduleModule.ViewModels
{
    public class OverlayAssignmentViewModel : BindableBase
    {
        private readonly ScheduledAssignmentDTO assignment;

        private readonly ICacheService cacheService;

        public OverlayAssignmentViewModel(ScheduledAssignmentDTO assignment, ICacheService cacheService)
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

        public DateTime StartTime { get { return assignment.StartTime; } }

        public DateTime EndTime { get { return assignment.EndTime; } }

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

        public OverlayAssignmentViewModelState State
        {
            get
            {
                return assignment.IsTemporary
                    ? OverlayAssignmentViewModelState.Temporary
                    : assignment.IsCompleted
                        ? OverlayAssignmentViewModelState.Completed
                        : OverlayAssignmentViewModelState.Uncompleted;
            }
        }
    }
    
    public enum OverlayAssignmentViewModelState
    {
        Temporary,
        Uncompleted,
        Completed
    }
}
