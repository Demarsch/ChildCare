using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Shared.PatientRecords.Services
{
    public interface IDiagnosService
    {
        IDisposableQueryable<DiagnosType> GetActualDiagnosTypes();
        IDisposableQueryable<DiagnosLevel> GetActualDiagnosLevels();
        IDisposableQueryable<Diagnosis> GetRecordDiagnos(int recordId, int? diagnosTypeId = null);
        IDisposableQueryable<DiagnosLevel> GetDiagnosLevelById(int id);
        IDisposableQueryable<DiagnosType> GetDiagnosTypeById(int id);
        IDisposableQueryable<DiagnosType> GetDiagnosTypeByOption(string option);
        IDisposableQueryable<Diagnosis> GetDiagnosById(int id);
        IDisposableQueryable<PersonDiagnos> GetPersonDiagnosById(int id);
        IDisposableQueryable<Complication> GetRootComplications();
        IDisposableQueryable<MKB> GetRootMKB();        
        IDisposableQueryable<MKB> GetMKBChildren(int parentId);
        IDisposableQueryable<MKB> GetMKBById(int[] ids);
        MKB GetMKBById(int id);        
        MKB GetMKBByCode(string code);

        bool DeleteDiagnos(int diagnosId, out string exception);
        bool DeletePersonDiagnos(int personDiagnosId, out string exception);

        int[] Save(int personId, int recordId, int diagnosTypeId, Diagnosis[] diagnosis, out string exception);
        
    }
}

