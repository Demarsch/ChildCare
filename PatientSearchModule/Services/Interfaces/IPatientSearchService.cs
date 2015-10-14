using Core.Data;
using Core.Misc;

namespace PatientSearchModule.Services
{
    public interface IPatientSearchService
    {
        IDisposableQueryable<Person> SearchPatients(string searchPattern);
    }
}
