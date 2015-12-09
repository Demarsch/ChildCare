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
        public string[] OpenFileDialog(bool multiSelect = false, string filter = null)
        {
            var files = new string[0];
            var dialog = new OpenFileDialog
                         {
                             Filter = filter ?? FileServiceFilters.Default,
                             Multiselect = false
                         };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                files = dialog.FileNames;
            }
            return files;
        }

        public void RunFile(string filePath)
        {
            Process.Start(filePath);
        }

        public void RunFile(string filePath, string fileServiceAction)
        {
            var info = new ProcessStartInfo(filePath)
            {
                Verb = fileServiceAction,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(info);
        }

        public string PrepareTempFileName(string folderPrefix, string fileTitle, string fileExtention)
        {
            var pathName = Path.GetTempPath();
            if (folderPrefix.Trim().Length > 0)
            {
                pathName += folderPrefix;
            }
            else
            {
                pathName += "Temp";
            }
            var i = 1;
            while (Directory.Exists(pathName + i))
            {
                i++;
            }
            pathName += i.ToString();
            var name = fileTitle;
            Path.GetInvalidFileNameChars().Where(x => name.Contains(x)).ToList().ForEach(x => name = name.Replace(x.ToString(), string.Empty));
            name = name.Trim();
            if (name.Length > 128)
            {
                name = name.Substring(0, 128).Trim();
            }
            else if (name.Length == 0)
            {
                name = "Документ";
            }
            Directory.CreateDirectory(pathName);
            pathName = Path.Combine(pathName, name);
            return Path.ChangeExtension(pathName, fileExtention);
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string FileNameInvalidChars(string fileName)
        {
            return Path.GetInvalidFileNameChars().Where(fileName.Contains).ToArray().Aggregate(string.Empty, (x, y) => string.Format("{0} {1}", x, y));
        }

        public ImageSource GetImageSourceFromBinaryData(byte[] source)
        {
            if (source == null)
            {
                return null;
            }
            var imageSourceConverter = new ImageSourceConverter();
            return (ImageSource)imageSourceConverter.ConvertFrom(source);
        }

        public byte[] GetBinaryDataFromImage(BitmapImage bitmap)
        {
            var data = new byte[0];
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var ms = new MemoryStream())
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
            {
                return bytes;
            }
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
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fs))
            {
                var byteLength = new FileInfo(filePath).Length;
                return binaryReader.ReadBytes((Int32)byteLength);
            }
        }

        public string GetFileFromBinaryData(byte[] binaryData, string extension)
        {
            var fileName = Path.GetTempFileName();
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(binaryData, 0, binaryData.Length);
            }
            // physically change file extension
            File.Move(fileName, Path.ChangeExtension(fileName, extension));
            // return new path
            return Path.ChangeExtension(fileName, extension);
        }

        public BitmapImage GetThumbnailForFile(byte[] binaryData, string extension)
        {
            var imageExt = new[] { "jpeg", "jpg", "png", "tiff", "bmp", "gif" };
            var wordExt = new[] { "doc", "docx" };
            var excelExt = new[] { "xls", "xlsx" };
            var pdfExt = new[] { "pdf" };

            if (wordExt.Contains(extension.ToLower()))
            {
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Word.png"));
            }
            if (excelExt.Contains(extension.ToLower()))
            {
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Excel.png"));
            }
            if (pdfExt.Contains(extension.ToLower()))
            {
                return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Pdf.png"));
            }
            if (imageExt.Contains(extension.ToLower()))
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
            return new BitmapImage(new Uri("pack://application:,,,/Core;Component/Resources/Images/File_Unknown.png"));
        }

        public Stream CreateStreamForFile(string fileName, bool readMode = true)
        {
            return new FileStream(fileName, readMode ? FileMode.Open : FileMode.Create, readMode ? FileAccess.Read : FileAccess.ReadWrite);
        }
    }
}
