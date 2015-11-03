﻿using Core.Data;
using Core.Data.Misc;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PatientInfoModule.Services
{
    public interface IDocumentService
    {
        int UploadDocument(Document document, out string msg);
        void DeleteDocumentById(int documentId);
        IDisposableQueryable<Document> GetDocumentById(int documentId);

        IDisposableQueryable<OuterDocumentType> GetOuterDocumentTypes(int? parentId);
        IDisposableQueryable<OuterDocumentType> GetOuterDocumentTypeById(int id);

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