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

    }
}
