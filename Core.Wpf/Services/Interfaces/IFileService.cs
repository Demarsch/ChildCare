using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Core.Wpf.Services
{
    public interface IFileService
    {        
        byte[] GetBinaryDataFromFile(string filePath);
        byte[] GetBinaryDataFromImage(BitmapImage bitmapImage);
        byte[] GetBinaryDataFromImage(BitmapEncoder encoder, ImageSource imageSource);

        string GetFileFromBinaryData(byte[] binaryData, string extension);

        BitmapImage GetThumbnailForFile(byte[] binaryData, string extension);
        ImageSource GetImageSourceFromBinaryData(byte[] source);

        void RunFile(string filePath);
        void RunFile(string filePath, string fileServiceAction);

        void DeleteFile(string filePath);

        string PrepareTempFileName(string folderPrefix, string fileTitle, string fileExtention);

        string FileNameInvalidChars(string fileName);

        string[] OpenFileDialog(bool multiSelect = false, string filter = null);

        Stream CreateStreamForFile(string fileName, bool readMode = true);

    }
}
