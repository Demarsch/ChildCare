using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;

namespace Shared.Patient.Services
{
    public interface IPatientAssignmentService
    {
        IDisposableQueryable<Assignment> GetAssignmentsQuery(int patientId);

        Task CancelAssignmentAsync(int assignmentId);

        IDisposableQueryable<ReportTemplate> GetAgreementDocuments();

        IDisposableQueryable<Person> GetPersonById(int id);

        string GetDBSettingValue(string parameter, bool useDisplayName = false);
    }
}
