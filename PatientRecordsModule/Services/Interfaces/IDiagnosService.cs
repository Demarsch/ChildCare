using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PatientRecordsModule.Services
{
    public interface IDiagnosService
    {
        IDisposableQueryable<DiagnosType> GetActualDiagnosTypes();
        IDisposableQueryable<DiagnosLevel> GetActualDiagnosLevels();
        IDisposableQueryable<Diagnosis> GetRecordDiagnos(int recordId, int? diagnosTypeId = null);
        IDisposableQueryable<DiagnosLevel> GetDiagnosLevelById(int id);
        IDisposableQueryable<Diagnosis> GetDiagnosById(int id);

        bool DeleteDiagnos(int diagnosId, out string exception);
        bool DeletePersonDiagnos(int personDiagnosId, out string exception);
    }
}
