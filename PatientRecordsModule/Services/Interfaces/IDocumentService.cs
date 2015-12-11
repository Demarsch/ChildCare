using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Shared.PatientRecords.Services
{
    public interface IDocumentService
    {
        IDisposableQueryable<Document> GetDocumentsByRecordId(int recordId);

        IDisposableQueryable<RecordDocument> GetRecordDocuments(int recordId, int assignmentId);

        bool SaveRecordDocument(RecordDocument recordDocument, out string message);

        bool DeleteRecordDocument(int documentId, out string message);

        IDisposableQueryable<Document> GetDocumentById(int documentId);

        string GetDocumentFile(int documentId);

        BitmapImage GetDocumentThumbnail(int documentId);

        Task<int> UploadDocument(Document document);

        Task<bool> SetRecordToDocuments(IDisposableQueryable<RecordDocument> recordDocumentsQuery);
    }
}
