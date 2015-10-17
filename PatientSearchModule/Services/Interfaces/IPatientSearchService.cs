using PatientSearchModule.Misc;

namespace PatientSearchModule.Services
{
    public interface IPatientSearchService
    {
        PatientSearchQuery GetPatientSearchQuery(string searchPattern);
    }
}
