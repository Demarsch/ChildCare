using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Shared.PatientRecords.Services
{
    public interface IRecordService
    {
        IDisposableQueryable<Assignment> GetAssignmentById(int id);
        IDisposableQueryable<Record> GetRecordById(int id);
        IDisposableQueryable<RecordType> GetRecordTypeById(int id);

        bool SaveRecordDocument(RecordDocument recordDocument, out string message);
        void UpdateMKBRecord(int recordId, string mkb);
        bool DeleteRecordDocument(int documentId, out string message);

        int SaveDefaultProtocol(DefaultProtocol defaultProtocol);
    }
}
