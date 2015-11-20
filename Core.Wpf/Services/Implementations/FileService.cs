using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Linq;
using Core.Wpf.Misc;

namespace Core.Wpf.Services
{
    public class FileService : IFileService
    {
        public FileService()
        {
           
        }

        public string[] OpenFileDialog(bool multiSelect = false, string filter = null)
        {
            string[] files = new string[0];
            OpenFileDialog dialog = new OpenFileDialog()
            { 
                Filter = filter ?? FileServiceFilters.Default,
                Multiselect = false
            };
            if (dialog.ShowDialog() == DialogResult.OK)
                files = dialog.FileNames;
            return files;
        }
        
        public void RunFile(string filePath)
        {
            Process.Start(filePath);
        }

        public void RunFile(string filePath, string fileServiceAction)
        {
            ProcessStartInfo info = new ProcessStartInfo(filePath)
            {
                Verb = fileServiceAction,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(info);
        }

        public string PrepareTempFileName(string folderPrefix, string fileTitle, string fileExtention)
        {
            string PathName = Path.GetTempPath();

            if (folderPrefix.Trim().Length > 0)
                PathName += folderPrefix;
            else
                PathName += "Temp";
            int i = 1;
            while (Directory.Exists(PathName + i)) i++;
            PathName += i.ToString();

            string name = fileTitle;
            Path.GetInvalidFileNameChars().Where(x => name.Contains(x)).ToList().ForEach(x => name = name.Replace(x.ToString(), string.Empty));
            name = name.Trim();
            if (name.Length > 128)
                name = name.Substring(0, 128).Trim();
            else if (name.Length == 0)
                name = "Документ";

            Directory.CreateDirectory(PathName);
            PathName = Path.Combine(PathName, name);
            return Path.ChangeExtension(PathName, fileExtention);
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public string FileNameInvalidChars(string fileName)
        {
            return Path.GetInvalidFileNameChars().Where(x => fileName.Contains(x)).ToArray().Aggregate(string.Empty, (x, y) => string.Format("{0} {1}", x, y));
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
            if (bitmapSource == null)
                return bytes;

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                bytes = stream.ToArray();
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

            if (WordExt.Contains(extension.ToLower()))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Word.png"));
            else if (ExcelExt.Contains(extension.ToLower()))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Excel.png"));
            else if (PDFExt.Contains(extension.ToLower()))
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Pdf.png"));
            else if (ImageExt.Contains(extension.ToLower()))
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

        public Stream CreateStreamForFile(string fileName, bool readMode = true)
        {
            return new FileStream(fileName, readMode ? FileMode.Open : FileMode.Create, readMode ? FileAccess.Read : FileAccess.ReadWrite);
        }

    }
}
