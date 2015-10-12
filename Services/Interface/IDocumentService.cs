using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DataLib;

namespace Core
{
    public interface IDocumentService
    {
        int UploadDocument(Document document, out string msg);
        void DeleteDocumentById(int documentId);    
        Document GetDocumentById(int documentId);
        
        ICollection<OuterDocumentType> GetOuterDocumentTypes(int? parentId);
        OuterDocumentType GetOuterDocumentTypeById(int id);
        OuterDocumentType GetParentOuterDocumentTypeById(int id);

        byte[] GetBinaryDataFromFile(string filePath);
        byte[] GetBinaryDataFromImage(BitmapImage bitmapImage);
        byte[] GetBinaryDataFromImage(BitmapEncoder encoder, ImageSource imageSource);
        BitmapImage GetThumbnailForFile(byte[] content, string extension);
        string GetFileFromBinaryData(byte[] content, string extension);
        ImageSource GetImageSourceFromBinaryData(byte[] source);

        void RunFile(string filePath);
        void DeleteFile(string filePath);
    }
}
