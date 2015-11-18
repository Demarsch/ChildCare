using System.Collections.Generic;
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
        void DeleteFile(string filePath);

        string[] OpenFileDialog(bool multiSelect = false);
    }
}
