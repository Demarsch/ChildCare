using System;

namespace Registry
{
    public class SamePatientAssignmentConflictException : AssignmentConflictException
    {
        public SamePatientAssignmentConflictException(string room, DateTime assginDateTime, string patient) : base(room, assginDateTime, patient)
        {
            message = string.Format("Указанное назначение конфликтует с другим назначением этого пациента на {0:HH:mm}, {1}", assginDateTime, room);
        }
    }
}