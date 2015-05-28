using System;

namespace Registry
{
    public class AssignmentConflictException : Exception
    {
        public string Room { get; private set; }

        public DateTime AssignDateTime { get; private set; }

        public string Patient { get; private set; }

        public AssignmentConflictException(string room, DateTime assginDateTime, string patient)
        {
            Room = room;
            AssignDateTime = assginDateTime;
            Patient = patient;
            message = string.Format("Указанное назначение конфликтует с назначением на {0:HH:mm}, пациент {1}, {2}", assginDateTime, patient, room);
        }

        protected string message;

        public override string Message { get { return message; } }
    }
}
