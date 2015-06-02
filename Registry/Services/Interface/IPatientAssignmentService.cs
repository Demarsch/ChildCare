using System;
using System.Collections.Generic;

namespace Registry
{
    public interface IPatientAssignmentService
    {
        ICollection<AssignmentDTO> GetAssignments(int patientId);

        ICollection<AssignmentDTO> GetActualAssignments(int patientId, DateTime date);

        AssignmentDTO GetAssignment(int assignmentId);
    }
}
