using System.Collections.Generic;

namespace Registry
{
    public interface IPatientAssignmentService
    {
        ICollection<AssignmentDTO> GetAssignments(int patientId);
    }
}
