using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;

namespace OrganizationContractsModule.Services
{
    public class DocumentService : IDocumentService
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

        public BitmapImage GetThumbnailForFile(int documentId)
        {
            var document = GetDocumentById(documentId).First();
            string[] ImageExt = new string[] { "jpeg", "jpg", "png", "tiff", "bmp", "gif" };
            string[] WordExt = new string[] { "doc", "docx" };
            string[] ExcelExt = new string[] { "xls", "xlsx" };
            string[] PDFExt = new string[] { "pdf" };

            if (WordExt.Contains(document.Extension))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Word.png"));
            else if (ExcelExt.Contains(document.Extension))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Excel.png"));
            else if (PDFExt.Contains(document.Extension))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Pdf.png"));
            else if (ImageExt.Contains(document.Extension))
            {
                using (var ms = new System.IO.MemoryStream(document.FileData))
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

        public string GetFileFromBinaryDocumentData(int documentId)
        {
            var document = GetDocumentById(documentId).First();
            string fileName = Path.GetTempFileName();
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                fs.Write(document.FileData, 0, document.FileData.Length);
            // physically change file extension
            File.Move(fileName, Path.ChangeExtension(fileName, document.Extension));
            // return new path
            return Path.ChangeExtension(fileName, document.Extension);
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

    }
}
