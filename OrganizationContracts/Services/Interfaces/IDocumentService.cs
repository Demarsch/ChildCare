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
        IDisposableQueryable<Document> GetRecordDocuments(int recordId);
        IDisposableQueryable<Document> GetDocumentById(int documentId);
        string GetDocumentFile(int documentId);
        BitmapImage GetDocumentThumbnail(int documentId);

        Task<int> UploadDocument(Document document);
    }
}
