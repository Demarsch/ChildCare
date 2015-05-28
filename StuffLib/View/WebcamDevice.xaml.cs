using AForge.Video;
using AForge.Video.DirectShow;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Video
{
    /// <summary>
    /// Interaction logic for Photoes.xaml
    /// </summary>
    public partial class WebcamDevice : UserControl
    {
        public static readonly DependencyProperty VideoPreviewWidthProperty = DependencyProperty.Register("VideoPreviewWidth", typeof(int), typeof(WebcamDevice));
        public static readonly DependencyProperty VideoPreviewHeightProperty = DependencyProperty.Register("VideoPreviewHeight", typeof(int), typeof(WebcamDevice));
        public static readonly DependencyProperty VideoSourceIdProperty = DependencyProperty.Register("VideoSourceId", typeof(string), typeof(WebcamDevice));
        public static readonly DependencyProperty SnapshotBitmapProperty = DependencyProperty.Register("SnapshotBitmap", typeof(BitmapImage), typeof(WebcamDevice));

        private ICommand TakeSnapshotCommand { get; set; }

        public WebcamDevice()
        {
            InitializeComponent();
            TakeSnapshotCommand = new RelayCommand(TakeSnapshot);
        }

        private void TakeSnapshot()
        {
            throw new NotImplementedException();
        }

        public int VideoPreviewWidth
        {
            get { return (int)GetValue(VideoPreviewWidthProperty); }
            set { SetValue(VideoPreviewWidthProperty, value); }
        }

        private static void VideoPreviewWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webcamDeviceView = d as WebcamDevice;
            webcamDeviceView.Width = (int)e.NewValue;
        }

        public int VideoPreviewHeight
        {
            get { return (int)GetValue(VideoPreviewHeightProperty); }
            set { SetValue(VideoPreviewHeightProperty, value); }
        }

        private static void VideoPreviewHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webcamDeviceView = d as WebcamDevice;
            webcamDeviceView.Height = (int)e.NewValue;
        }

        public string VideoSourceId 
        {
            get { return (string)GetValue(VideoSourceIdProperty); }
            set { SetValue(VideoSourceIdProperty, value); }
        }

        private static void VideoSourceIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webcamDeviceView = d as WebcamDevice;
            webcamDeviceView.VideoSourcePlayer.VideoSource = new VideoCaptureDevice((string)e.NewValue);
            webcamDeviceView.VideoSourcePlayer.Start();
        }

        public string SnapshotBitmap
        {
            get { return (string)GetValue(SnapshotBitmapProperty); }
            set { SetValue(SnapshotBitmapProperty, value); }
        }

        private static void SnapshotBitmapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webcamDeviceView = d as WebcamDevice;
            // ToDO: Set VideoSourceId
        }
    }
}
