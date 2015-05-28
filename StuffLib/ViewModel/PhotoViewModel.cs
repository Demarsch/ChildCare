using AForge.Video.DirectShow;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StuffLib
{
    class PhotoViewModel : ObservableObject
    {
        public IEnumerable<MediaInformation> MediaDeviceList
        {
            get
            {
                var filterVideoDeviceCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                return (from FilterInfo filterInfo in filterVideoDeviceCollection select new MediaInformation { DisplayName = filterInfo.Name, UsbId = filterInfo.MonikerString }).ToList();
            }
        }

        private string selectedVideoDeviceId = string.Empty;
        public string SelectedVideoDeviceId
        {
            get { return selectedVideoDeviceId; }
            set { Set(() => SelectedVideoDeviceId, ref selectedVideoDeviceId, value); }
        }

        private int videoPreviewWidth = 400;
        public int VideoPreviewWidth
        {
            get { return videoPreviewWidth; }
            set { Set(() => VideoPreviewWidth, ref videoPreviewWidth, value); }
        }

        private int videoPreviewHeight = 600;
        public int VideoPreviewHeight
        {
            get { return videoPreviewHeight; }
            set { Set(() => VideoPreviewHeight, ref videoPreviewHeight, value); }
        }

        private BitmapImage snapshotBitmap;
        public BitmapImage SnapshotBitmap
        {
            get { return snapshotBitmap; }
            set { Set(() => SnapshotBitmap, ref snapshotBitmap, value); }
        }
    }
}
