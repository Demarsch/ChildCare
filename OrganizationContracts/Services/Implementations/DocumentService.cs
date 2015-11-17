using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;

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

        public IDisposableQueryable<Document> GetRecordDocuments(int recordId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Document>(context.Set<RecordDocument>().Where(x => x.RecordId == recordId).Select(x => x.Document), context);
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
