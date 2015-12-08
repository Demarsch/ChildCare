using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;
using Shared.Patient.Views;

namespace Shared.Patient.ViewModels
{
    public class PhotoViewModel : BindableBase, IDialogViewModel
    {
        /// <summary>
        /// Instance of relay command for snapshot command.
        /// </summary>
        private readonly DelegateCommand snapshotCommand;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PhotoViewModel()
        {
            MediaDeviceList = WebcamDevice.GetVideoDevices;
            VideoPreviewWidth = 240;
            VideoPreviewHeight = 320;
            SelectedVideoDeviceId = string.Empty;
            var mediaDevice = MediaDeviceList.FirstOrDefault();
            if (mediaDevice != null)
            {
                SelectedVideoDeviceId = mediaDevice.UsbId;
            }
            CloseCommand = new DelegateCommand<bool?>(Close);
            snapshotCommand = new DelegateCommand(OnSnapshot);
        }

        /// <summary>
        /// Video preview width.
        /// </summary>
        private int videoPreviewWidth;
        public int VideoPreviewWidth
        {
            get { return videoPreviewWidth; }
            set { SetProperty(ref videoPreviewWidth, value); }
        }

        /// <summary>
        /// Video preview height.
        /// </summary>
        private int videoPreviewHeight;
        public int VideoPreviewHeight
        {
            get { return videoPreviewHeight; }
            set { SetProperty(ref videoPreviewHeight, value); }
        }

        /// <summary>
        /// Selected video device.
        /// </summary>
        private string selectedVideoDeviceId;
        public string SelectedVideoDeviceId
        {
            get { return selectedVideoDeviceId; }
            set { SetProperty(ref selectedVideoDeviceId, value); }
        }

        /// <summary>
        /// Snapshot taken.
        /// </summary>
        private ImageSource snapshotTaken;
        public ImageSource SnapshotTaken
        {
            get { return snapshotTaken; }
            set { SetProperty(ref snapshotTaken, value); }
        }

        /// <summary>
        /// Byte array of snapshot image.
        /// </summary>
        private Bitmap snapshotBitmap;
        public Bitmap SnapshotBitmap
        {
            get { return snapshotBitmap; }
            set { SetProperty(ref snapshotBitmap, value); }
        }

        /// <summary>
        /// List of media information device available.
        /// </summary>
        private IEnumerable<MediaInformation> mediaDeviceList;
        public IEnumerable<MediaInformation> MediaDeviceList
        {
            get { return mediaDeviceList; }
            set { SetProperty(ref mediaDeviceList, value); }
        }
        /// <summary>
        /// Gets instance of relay command for snapshot command.
        /// </summary>
        public ICommand SnapshotCommand { get { return snapshotCommand; } }
        /// <summary>
        /// Event handler for on take snapshot command click.
        /// </summary>
        private void OnSnapshot()
        {
            SnapshotTaken = ConvertToImageSource(SnapshotBitmap);
        }
        /// <summary>
        /// The convert to image source.
        /// </summary>
        /// <param name="bitmap"> The bitmap. </param>
        /// <returns> The <see cref="object"/>. </returns>
        public static ImageSource ConvertToImageSource(Bitmap bitmap)
        {
            var imageSourceConverter = new ImageSourceConverter();
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                var snapshotBytes = memoryStream.ToArray();
                return (ImageSource)imageSourceConverter.ConvertFrom(snapshotBytes);
            }
        }

        #region Implement IDialogViewModel

        public string Title
        {
            get { return "Сделать фото"; }
        }

        public string ConfirmButtonText
        {
            get { return "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; set; }

        private void Close(bool? validate)
        {
            OnCloseRequested(new ReturnEventArgs<bool>(validate.GetValueOrDefault()));
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}
