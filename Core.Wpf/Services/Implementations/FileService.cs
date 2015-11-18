using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Linq;

namespace Core.Wpf.Services
{
    public class FileService : IFileService
    {
        public FileService()
        {
           
        }

        public string[] OpenFileDialog(bool multiSelect = false)
        {
            string[] files = new string[0];
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*|Office Files|*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx|Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|Text files (*.txt)|*.txt";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                files = dialog.FileNames;
            return files;
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

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public ImageSource GetImageSourceFromBinaryData(byte[] source)
        {
            var imageSourceConverter = new ImageSourceConverter();
            return (ImageSource)imageSourceConverter.ConvertFrom(source);
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


        public string GetFileFromBinaryData(byte[] binaryData, string extension)
        {
            string fileName = Path.GetTempFileName();
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                fs.Write(binaryData, 0, binaryData.Length);
            // physically change file extension
            File.Move(fileName, Path.ChangeExtension(fileName, extension));
            // return new path
            return Path.ChangeExtension(fileName, extension);
        }

        public BitmapImage GetThumbnailForFile(byte[] binaryData, string extension)
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
                using (var ms = new MemoryStream(binaryData))
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
