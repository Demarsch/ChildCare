using System;
using Prism.Mvvm;

namespace Shared.Patient.ViewModels
{
    public class PatientAssignmentViewModel : BindableBase
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private DateTime assignDateTime;

        public DateTime AssignDateTime
        {
            get { return assignDateTime; }
            set { SetProperty(ref assignDateTime, value); }
        }

        private bool isCompleted;

        public bool IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }

        private string room;

        public string Room
        {
            get { return room; }
            set { SetProperty(ref room, value); }
        }

        private string recordType;

        public string RecordType
        {
            get { return recordType; }
            set { SetProperty(ref recordType, value); }
        }
    }
}
