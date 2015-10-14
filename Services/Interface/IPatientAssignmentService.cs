using System;
using System.Collections.Generic;

namespace Core
{
    public interface IPatientAssignmentService
    {
        ICollection<AssignmentScheduleDTO> GetAssignments(int patientId);

        ICollection<AssignmentScheduleDTO> GetActualAssignments(int patientId, DateTime date);

        AssignmentScheduleDTO GetAssignment(int assignmentId, int patientId);
    }
}
