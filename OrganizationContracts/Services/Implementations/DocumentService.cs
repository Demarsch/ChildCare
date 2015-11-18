using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using System.IO;
using Core.Wpf.Services;

namespace OrganizationContractsModule.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDbContextProvider contextProvider;
        private readonly IFileService fileService;

        public DocumentService(IDbContextProvider contextProvider, IFileService fileService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (fileService == null)
            {
                throw new ArgumentNullException("fileService");
            }
            this.contextProvider = contextProvider;
            this.fileService = fileService;
        }

        public IDisposableQueryable<Document> GetDocumentById(int documentId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Document>(context.Set<Document>().Where(x => x.Id == documentId), context);
        }

        public IDisposableQueryable<Document> GetRecordDocuments(int recordId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Document>(context.Set<RecordDocument>().Where(x => x.RecordId == recordId).Select(x => x.Document), context);
        }

        public string GetDocumentFile(int documentId)
        {
            var document = GetDocumentById(documentId).First();
            return fileService.GetFileFromBinaryData(document.FileData, document.Extension);
        }

        public BitmapImage GetDocumentThumbnail(int documentId)
        {
            var document = GetDocumentById(documentId).First();
            return fileService.GetThumbnailForFile(document.FileData, document.Extension);
        }
    }
}
