using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataLib;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Core
{
    public class DocumentService : IDocumentService
    {
        private IDataContextProvider provider;

        public DocumentService(IDataContextProvider Provider)
        {
            provider = Provider;
        }

        public int UploadDocument(Document document, out string msg)
        {
            string exception = string.Empty;
            try
            {
                using (var db = provider.GetNewDataContext())
                {
                    var dbDocument = document.Id > 0 ? db.GetById<Document>(document.Id) : new Document();
                    dbDocument.FileName = document.FileName;
                    dbDocument.DocumentFromDate = document.DocumentFromDate;
                    dbDocument.Description = document.Description;
                    dbDocument.DisplayName = document.DisplayName;
                    dbDocument.Extension = document.Extension;
                    dbDocument.FileData = document.FileData;
                    dbDocument.FileSize = document.FileSize;
                    dbDocument.UploadDate = document.UploadDate;
                    if (dbDocument.Id == 0)
                        db.Add<Document>(dbDocument);
                    db.Save();
                    msg = exception;
                    return dbDocument.Id;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return 0;
            }
        }

        public void DeleteDocumentById(int documentId)
        {
            using (var db = provider.GetNewDataContext())
            {
                var document = db.GetById<Document>(documentId);
                db.Remove<Document>(document);
                db.Save();
            }
        }

        public Document GetDocumentById(int documentId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<Document>(documentId);
            }
        }

        public ICollection<OuterDocumentType> GetOuterDocumentTypes(int? parentId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<OuterDocumentType>().Where(x => (parentId.HasValue ? x.ParentId == parentId : x.ParentId.HasValue)).ToArray();
            }
        }

        public OuterDocumentType GetOuterDocumentTypeById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<OuterDocumentType>(id);
            }
        }

        public OuterDocumentType GetParentOuterDocumentTypeById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<OuterDocumentType>(id).OuterDocumentType1;
            }
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

        public BitmapImage GetThumbnailForFile(Byte[] content, string extension)
        {
            string[] ImageExt = new string[]{ "jpeg", "jpg", "png", "tiff", "bmp", "gif" };
            string[] WordExt = new string[]{ "doc", "docx" };
            string[] ExcelExt = new string[]{ "xls", "xlsx" };
            string[] PDFExt = new string[]{ "pdf" };

            if (WordExt.Contains(extension))
                return new BitmapImage(new Uri("pack://application:,,,/Resources;component/Images/File_Word.png"));
            else if (ExcelExt.Contains(extension))
                return new BitmapImage(new Uri("pack://application:,,,/Resources;component/Images/File_Excel.png"));
            else if (PDFExt.Contains(extension))
                return new BitmapImage(new Uri("pack://application:,,,/Resources;component/Images/File_Pdf.png"));
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
                return new BitmapImage(new Uri("pack://application:,,,/Resources;component/Images/File_Unknown.png"));
        }
    }
}
