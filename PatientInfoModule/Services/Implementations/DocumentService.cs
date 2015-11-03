using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.IO;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Imaging;

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

        public int UploadDocument(Document document, out string msg)
        {
            string exception = string.Empty;
            try
            {
                var context = contextProvider.CreateNewContext();
                /*var dbDocument = document.Id > 0 ? db.GetById<Document>(document.Id) : new Document();
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
                return dbDocument.Id;*/
                msg = exception;
                return 0;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return 0;
            }
        }

        public void DeleteDocumentById(int documentId)
        {
            var context = contextProvider.CreateNewContext();
            /*var document = db.GetById<Document>(documentId);
            db.Remove<Document>(document);
            db.Save();*/
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
