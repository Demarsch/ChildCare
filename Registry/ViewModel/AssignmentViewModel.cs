using System;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class AssignmentViewModel : ObservableObject
    {
        private readonly AssignmentDTO assignment;

        public AssignmentViewModel(AssignmentDTO assignment)
        {
            if (assignment == null)
                throw new ArgumentNullException("assignment");
            this.assignment = assignment;
        }

        public DateTime AssignDateTime { get { return assignment.AssignDateTime; } }

        public bool IsCancelled { get { return assignment.IsCanceled; } }

        public bool IsCompleted { get { return assignment.IsCompleted; } }
        //TODO: use reference service
        public string Room { get { return assignment.RoomId.ToString(); } }
        //TODO: use refrence service
        public string RecordType { get { return "Укол в зад"; } }
    }
}