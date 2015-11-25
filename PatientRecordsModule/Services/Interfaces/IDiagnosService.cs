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
    }
}
