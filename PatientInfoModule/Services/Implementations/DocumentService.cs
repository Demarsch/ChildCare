using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;
using Core.Wpf.Services;
using System.Windows.Media.Imaging;

namespace PatientInfoModule.Services
{
    public class DocumentService: IDocumentService
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

        public async Task<int> UploadDocument(Document document)
        {            
            using (var db = contextProvider.CreateNewContext())
            {
                var saveDocument = document.Id == SpecialValues.NewId ? new Document() : db.Set<Document>().First(x => x.Id == document.Id);
                saveDocument.FileName = document.FileName;
                saveDocument.DocumentFromDate = document.DocumentFromDate;
                saveDocument.Description = document.Description;
                saveDocument.DisplayName = document.DisplayName;
                saveDocument.Extension = document.Extension;
                saveDocument.FileData = document.FileData;
                saveDocument.FileSize = document.FileSize;
                saveDocument.UploadDate = document.UploadDate;
                db.Entry<Document>(saveDocument).State = saveDocument.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                await db.SaveChangesAsync();
                return saveDocument.Id;
            }            
        }

        public void DeleteDocumentById(int documentId)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var document = db.Set<Document>().First(x => x.Id == documentId);
                db.Entry(document).State = EntityState.Deleted;
                db.SaveChanges();
            }
        }

        public IDisposableQueryable<Document> GetDocumentById(int documentId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Document>(context.Set<Document>().Where(x => x.Id == documentId), context);
        }

        public IDisposableQueryable<OuterDocumentType> GetOuterDocumentTypes(int? parentId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<OuterDocumentType>(context.Set<OuterDocumentType>().Where(x => (parentId.HasValue ? x.ParentId == parentId : x.ParentId.HasValue)), context);
        }

        public IDisposableQueryable<OuterDocumentType> GetOuterDocumentTypeById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<OuterDocumentType>(context.Set<OuterDocumentType>().Where(x => x.Id == id), context);
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
