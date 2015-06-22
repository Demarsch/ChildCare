using AForge.Video.DirectShow;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Core;
using System.Windows.Navigation;

namespace StuffLib
{
    public class PhotoViewModel : ObservableObject, IDialogViewModel
    {
        /// <summary>
        /// Instance of relay command for loaded event.
        /// </summary>
        private RelayCommand loadedCommand;

        /// <summary>
        /// Instance of relay command for snapshot command.
        /// </summary>
        private RelayCommand snapshotCommand;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PhotoViewModel()
        {
            this.MediaDeviceList = Video.WebcamDevice.GetVideoDevices;
            this.VideoPreviewWidth = 240;
            this.VideoPreviewHeight = 320;
            this.SelectedVideoDeviceId = string.Empty;
            if (this.MediaDeviceList.Any())
                this.SelectedVideoDeviceId = this.MediaDeviceList.First().UsbId;
            CloseCommand = new RelayCommand<object>(x => Close((bool?)x));
        }

        /// <summary>
        /// Video preview width.
        /// </summary>
        private int videoPreviewWidth;
        public int VideoPreviewWidth
        {
            get { return videoPreviewWidth; }
            set { Set(() => VideoPreviewWidth, ref videoPreviewWidth, value); }
        }

        /// <summary>
        /// Video preview height.
        /// </summary>
        private int videoPreviewHeight;
        public int VideoPreviewHeight
        {
            get { return this.videoPreviewHeight; }
            set { Set(() => VideoPreviewHeight, ref videoPreviewHeight, value); }
        }

        /// <summary>
        /// Selected video device.
        /// </summary>
        private string selectedVideoDeviceId;
        public string SelectedVideoDeviceId
        {
            get { return this.selectedVideoDeviceId; }
            set { Set(() => SelectedVideoDeviceId, ref selectedVideoDeviceId, value); }
        }

        /// <summary>
        /// Snapshot taken.
        /// </summary>
        private ImageSource snapshotTaken;
        public ImageSource SnapshotTaken
        {
            get { return this.snapshotTaken; }
            set { Set(() => SnapshotTaken, ref snapshotTaken, value); }
        }

        /// <summary>
        /// Byte array of snapshot image.
        /// </summary>
        private Bitmap snapshotBitmap;
        public Bitmap SnapshotBitmap
        {
            get { return this.snapshotBitmap; }
            set { Set(() => SnapshotBitmap, ref snapshotBitmap, value); }
        }

        /// <summary>
        /// List of media information device available.
        /// </summary>
        private IEnumerable<MediaInformation> mediaDeviceList;
        public IEnumerable<MediaInformation> MediaDeviceList
        {
            get { return this.mediaDeviceList; }
            set { Set(() => MediaDeviceList, ref mediaDeviceList, value); }
        }

        /// <summary>
        /// Gets instance of relay command of type windows loaded.
        /// </summary>
        //public RelayCommand LoadedCommand
        //{
        //    get
        //    {
        //        return this.loadedCommand ?? (this.loadedCommand = new RelayCommand(this.OnWindowLoaded));
        //    }
        //}

        /// <summary>
        /// Gets instance of relay command for snapshot command.
        /// </summary>
        public RelayCommand SnapshotCommand
        {
            get
            {
                return this.snapshotCommand ?? (this.snapshotCommand = new RelayCommand(this.OnSnapshot));
            }
        }

        /// <summary>
        /// Event handler for windows loaded event.
        /// </summary>
        //private void OnWindowLoaded()
        //{

        //}

        /// <summary>
        /// Event handler for on take snapshot command click.
        /// </summary>
        private void OnSnapshot()
        {
            this.SnapshotTaken = ConvertToImageSource(this.SnapshotBitmap);
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

        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        public RelayCommand<object> CloseCommand { get; set; }

        private void Close(bool? validate)
        {
            OnCloseRequested(new ReturnEventArgs<bool>(true));
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
