using System.Collections.Generic;
using DataLib;
using System;
using System.Drawing;
using System.Windows.Media.Imaging;

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
        BitmapImage GetImageFromBinaryData(byte[] binaryData);
        string GetFileFromBinaryData(byte[] content, string extension);

        void RunFile(string filePath);
        void DeleteFile(string filePath);
    }
}
