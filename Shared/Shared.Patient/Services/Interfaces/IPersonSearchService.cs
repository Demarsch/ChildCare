using Shared.Patient.Misc;

namespace Shared.Patient.Services
{
    public interface IPersonSearchService
    {
        PersonSearchQuery GetPatientSearchQuery(string searchPattern);
    }
}
