﻿using Core.Data;
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

        BitmapImage GetThumbnailForFile(byte[] content, string extension);
    }
}