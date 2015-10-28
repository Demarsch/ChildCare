using Core.Data;
using Core.Data.Misc;

namespace PatientRecordsModule.Services
{
    public interface IPatientRecordsService
    {
        IDisposableQueryable<Person> GetPersonQuery(int personId);

        IDisposableQueryable<Record> GetPersonRecordsQuery(int personId);

        IDisposableQueryable<Assignment> GetPersonRootAssignmentsQuery(int personId);

        IDisposableQueryable<Visit> GetPersonVisitsQuery(int personId);

        IDisposableQueryable<Assignment> GetVisitsChildAssignmentsQuery(int visitId);

        IDisposableQueryable<Record> GetVisitsChildRecordsQuery(int visitId);

        IDisposableQueryable<Assignment> GetRecordsChildAssignmentsQuery(int recordId);

        IDisposableQueryable<Record> GetRecordsChildRecordsQuery(int recordId);

        IDisposableQueryable<Assignment> GetAssignmentsChildAssignmentsQuery(int assignmentId);

    }
}
