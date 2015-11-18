using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OrganizationContractsModule.Services
{
    public interface IRecordService
    {
        IDisposableQueryable<Record> GetRecordById(int id);
        IDisposableQueryable<RecordType> GetRecordTypeById(int id);

        bool SaveRecordDocument(RecordDocument recordDocument);
        void DeleteRecordDocument(int documentId);
    }
}
