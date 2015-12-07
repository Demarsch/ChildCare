﻿using Core.Data;
using Core.Data.Misc;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PatientInfoModule.Services
{
    public interface IDocumentService
    {
        Task<int> UploadDocumentAsync(Document document);

        void DeleteDocumentById(int documentId);
        
        IDisposableQueryable<Document> GetDocumentById(int documentId);
        
        IDisposableQueryable<OuterDocumentType> GetOuterDocumentTypes(int? parentId);
        
        IDisposableQueryable<OuterDocumentType> GetOuterDocumentTypeById(int id);
        
        string GetDocumentFile(int documentId);
        
        BitmapImage GetDocumentThumbnail(int documentId);
    }
}
