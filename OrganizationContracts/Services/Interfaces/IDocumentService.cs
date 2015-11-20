using Core.Data;
using Core.Data.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OrganizationContractsModule.Services
{
    public interface IDocumentService
    {
        IDisposableQueryable<Document> GetDocumentsByRecordId(int recordId);
        IDisposableQueryable<RecordDocument> GetRecordDocuments(int? recordId, int? assignmentId);
        IDisposableQueryable<Document> GetDocumentById(int documentId);
        string GetDocumentFile(int documentId);
        BitmapImage GetDocumentThumbnail(int documentId);

        Task<int> UploadDocument(Document document);

        Task<bool> SetRecordToDocuments(IDisposableQueryable<RecordDocument> recordDocumentsQuery);
    }
}
