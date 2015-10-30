using Core.Data;
using Core.Data.Misc;
using System;

namespace PatientInfoModule.Services
{
    public interface IAssignmentService
    {
        IDisposableQueryable<Assignment> GetPersonAssignments(int patientId);

        
    }
}
