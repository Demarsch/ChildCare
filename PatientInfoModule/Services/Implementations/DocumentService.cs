﻿using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.IO;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;

namespace PatientInfoModule.Services
{
    public class DocumentService: IDocumentService
    {
        private readonly IDbContextProvider contextProvider;

        public DocumentService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
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
        
        public byte[] GetBinaryDataFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    long byteLength = new FileInfo(filePath).Length;
                    return binaryReader.ReadBytes((Int32)byteLength);
                }
            }
            //return File.ReadAllBytes(filePath);
        }

        public string GetFileFromBinaryData(byte[] content, string extension)
        {
            string fileName = Path.GetTempFileName();
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                fs.Write(content, 0, content.Length);
            // physically change file extension
            File.Move(fileName, Path.ChangeExtension(fileName, extension));
            // return new path
            return Path.ChangeExtension(fileName, extension); ;
        }

        public ImageSource GetImageSourceFromBinaryData(byte[] source)
        {
            var imageSourceConverter = new ImageSourceConverter();
            return (ImageSource)imageSourceConverter.ConvertFrom(source);
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public void RunFile(string filePath)
        {
            Process prc = new Process();
            prc.StartInfo.FileName = filePath;
            prc.EnableRaisingEvents = true;
            prc.Exited += (sender, e) =>
            {
                try
                {
                    File.Delete(filePath);
                }
                catch { }
            };
            prc.Start();
        }

        public byte[] GetBinaryDataFromImage(BitmapImage bitmap)
        {
            byte[] data = new byte[0];
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

        public byte[] GetBinaryDataFromImage(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;
            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return bytes;
        }

        public BitmapImage GetThumbnailForFile(Byte[] content, string extension)
        {
            string[] ImageExt = new string[] { "jpeg", "jpg", "png", "tiff", "bmp", "gif" };
            string[] WordExt = new string[] { "doc", "docx" };
            string[] ExcelExt = new string[] { "xls", "xlsx" };
            string[] PDFExt = new string[] { "pdf" };
        
            if (WordExt.Contains(extension))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Word.png"));
            else if (ExcelExt.Contains(extension))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Excel.png"));
            else if (PDFExt.Contains(extension))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Pdf.png"));
            else if (ImageExt.Contains(extension))
            {
                using (var ms = new System.IO.MemoryStream(content))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }
            else
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Unknown.png"));
        }
    }
}
