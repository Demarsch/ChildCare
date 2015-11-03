using Core.Data;
using Core.Data.Misc;

namespace PatientInfoModule.Services
{
    public interface IAssignmentService
    {
        IDisposableQueryable<Assignment> GetPersonAssignments(int patientId);

        
    }
}
