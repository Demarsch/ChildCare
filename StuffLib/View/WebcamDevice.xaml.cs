using AForge.Video;
using AForge.Video.DirectShow;
using GalaSoft.MvvmLight.Command;
using StuffLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        public static readonly DependencyProperty VideoPreviewWidthProperty = DependencyProperty.Register("VideoPreviewWidth", typeof(int), typeof(WebcamDevice), new PropertyMetadata(VideoPreviewWidthPropertyChangedCallback));
        public static readonly DependencyProperty VideoPreviewHeightProperty = DependencyProperty.Register("VideoPreviewHeight", typeof(int), typeof(WebcamDevice), new PropertyMetadata(VideoPreviewHeightPropertyChangedCallback));
        public static readonly DependencyProperty VideoSourceIdProperty = DependencyProperty.Register("VideoSourceId", typeof(string), typeof(WebcamDevice),
            new PropertyMetadata(string.Empty, VideoSourceIdPropertyChangedCallback, VideoSourceIdPropertyCoherceValueChanged));
        public static readonly DependencyProperty SnapshotBitmapProperty = DependencyProperty.Register("SnapshotBitmap", typeof(Bitmap), typeof(WebcamDevice), new PropertyMetadata(SnapshotBitmapPropertyChangedCallback));
        public static readonly DependencyProperty TakeSnapshotProperty = DependencyProperty.RegisterAttached("TakeSnapshot", typeof(ICommand), typeof(WebcamDevice), new PropertyMetadata(default(TakeSnapshotCommand)));
        /// <summary>
        /// Instance of video capture device.
        /// </summary>
        private VideoCaptureDevice videoCaptureDevice;

        /// <summary>
        /// The is video source initialized.
        /// </summary>
        private bool isVideoSourceInitialized;

        public WebcamDevice()
        {
            InitializeComponent();
            //// Subcribe to dispatcher shutdown event and dispose all used resources gracefully.
            this.Dispatcher.ShutdownStarted += this.DispatcherShutdownStarted;
            //// Initialize take snapshot command.
            this.TakeSnapshot = new TakeSnapshotCommand(this.TakeSnapshotCallback);
        }

        private void TakeSnapshotCallback()
        {
            try
            {
                var playerPoint = new System.Drawing.Point();

                //// Get the position of the source video device player.
                if (string.IsNullOrWhiteSpace(this.VideoSourceId))
                {
                    var noVideoDeviceSourcePoint = this.NoVideoSourceGrid.PointToScreen(new System.Windows.Point(0, 0));
                    playerPoint.X = (int)noVideoDeviceSourcePoint.X;
                    playerPoint.Y = (int)noVideoDeviceSourcePoint.Y;
                }
                else
                {
                    playerPoint = this.VideoSourcePlayer.PointToScreen(new System.Drawing.Point(this.VideoSourcePlayer.ClientRectangle.X, this.VideoSourcePlayer.ClientRectangle.Y));
                }

                if (double.IsNaN(this.VideoPreviewWidth) || double.IsNaN(this.VideoPreviewHeight))
                {
                    using (var bitmap = new Bitmap((int)this.VideoSourceWindowsFormsHost.ActualWidth, (int)this.VideoSourceWindowsFormsHost.ActualHeight))
                    {
                        using (var graphicsFromImage = Graphics.FromImage(bitmap))
                        {
                            graphicsFromImage.CopyFromScreen(playerPoint, System.Drawing.Point.Empty, new System.Drawing.Size((int)this.VideoSourceWindowsFormsHost.ActualWidth, (int)this.VideoSourceWindowsFormsHost.ActualHeight));
                        }

                        this.SnapshotBitmap = new Bitmap(bitmap);
                    }
                }
                else
                {
                    using (var bitmap = new Bitmap((int)this.VideoPreviewWidth, (int)this.VideoPreviewHeight))
                    {
                        using (var graphicsFromImage = Graphics.FromImage(bitmap))
                        {
                            graphicsFromImage.CopyFromScreen(playerPoint, System.Drawing.Point.Empty, new System.Drawing.Size((int)this.VideoPreviewWidth, (int)this.VideoPreviewHeight));
                        }

                        this.SnapshotBitmap = new Bitmap(bitmap);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Error occurred while trying to take snapshot from currently selected source video device", exception);
            }
        }

        public int VideoPreviewWidth
        {
            get { return (int)GetValue(VideoPreviewWidthProperty); }
            set { SetValue(VideoPreviewWidthProperty, value); }
        }

        private static void VideoPreviewWidthPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webcamDeviceView = d as WebcamDevice;
            webcamDeviceView.Width = (int)e.NewValue;
            webcamDeviceView.VideoSourcePlayer.Width = (int)e.NewValue;
        }

        public int VideoPreviewHeight
        {
            get { return (int)GetValue(VideoPreviewHeightProperty); }
            set { SetValue(VideoPreviewHeightProperty, value); }
        }

        private static void VideoPreviewHeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webcamDeviceView = d as WebcamDevice;
            webcamDeviceView.Height = (int)e.NewValue;
            webcamDeviceView.VideoSourcePlayer.Width = (int)e.NewValue;
        }

        public string VideoSourceId
        {
            get { return (string)GetValue(VideoSourceIdProperty); }
            set { SetValue(VideoSourceIdProperty, value); }
        }

        /// <summary>
        /// Gets or sets take snapshot command.
        /// </summary>
        public TakeSnapshotCommand TakeSnapshot
        {
            get { return (TakeSnapshotCommand)GetValue(TakeSnapshotProperty); }
            set { SetValue(TakeSnapshotProperty, value); }
        }

        private static void VideoSourceIdPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var oldValue = e.OldValue as string;
            var newValue = e.NewValue as string;
            var webCamDevice = d as WebcamDevice;
            if (null == webCamDevice)
            {
                return;
            }

            if (null == e.NewValue)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(newValue))
            {
                if (!string.IsNullOrWhiteSpace(oldValue))
                {
                    webCamDevice.InitializeVideoDevice(oldValue);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(oldValue))
                {
                    webCamDevice.InitializeVideoDevice(newValue);
                }
                else
                {
                    if (oldValue != newValue)
                    {
                        webCamDevice.isVideoSourceInitialized = false;
                    }

                    webCamDevice.InitializeVideoDevice(oldValue.Equals(newValue) ? oldValue : newValue);
                }
            }
        }

        /// <summary>
        /// Event handler for video device Id value changed event.
        /// </summary>
        /// <param name="dependencyObject">Instance of dependency object.</param>
        /// <param name="basevalue">Base value.</param>
        /// <returns>Return base value / NULL or the new Id value of the video device source.</returns>
        private static object VideoSourceIdPropertyCoherceValueChanged(DependencyObject dependencyObject, object basevalue)
        {
            var baseValueStringFormat = Convert.ToString(basevalue, CultureInfo.InvariantCulture);
            var availableMediaList = GetVideoDevices;
            var mediaInfos = availableMediaList as IList<MediaInformation> ?? availableMediaList.ToList();
            if (string.IsNullOrEmpty(baseValueStringFormat) || !mediaInfos.Any())
            {
                return null;
            }

            var filteredVideoDevice = mediaInfos.FirstOrDefault(item => item.UsbId.Equals(baseValueStringFormat));
            return null != filteredVideoDevice ? filteredVideoDevice.UsbId : baseValueStringFormat;
        }

        public Bitmap SnapshotBitmap
        {
            get { return (Bitmap)GetValue(SnapshotBitmapProperty); }
            set { SetValue(SnapshotBitmapProperty, value); }
        }

        private static void SnapshotBitmapPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //// NOTE: Created to make the dependency property bindable from view-model.
        }


        /// <summary>
        /// Initialize video device.
        /// </summary>
        /// <param name="videoDeviceSourceId">Video device source Id.</param>
        /// <exception cref="InvalidOperationException">Throws invalid operation exception if video device source setup fails.</exception>
        private void InitializeVideoDevice(string videoDeviceSourceId)
        {
            if (this.isVideoSourceInitialized)
            {
                return;
            }

            var errorAction = new Action(() => this.SetVideoPlayer(false, "Невозможно подключиться к видео устройсту"));
            this.ReleaseVideoDevice();
            if (string.IsNullOrEmpty(videoDeviceSourceId))
            {
                return;
            }

            if (videoDeviceSourceId.StartsWith("Message:", StringComparison.OrdinalIgnoreCase))
            {
                var splitString = videoDeviceSourceId.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitString.Length == 2)
                {
                    this.SetVideoPlayer(false, splitString[1]);
                }
                else
                {
                    this.SetVideoPlayer(false);
                }
            }
            else
            {
                try
                {
                    if (!GetVideoDevices.Any(item => item.UsbId.Equals(videoDeviceSourceId)))
                    {
                        return;
                    }

                    this.videoCaptureDevice = new VideoCaptureDevice(videoDeviceSourceId);
                    this.VideoSourcePlayer.VideoSource = this.videoCaptureDevice;
                    this.VideoSourcePlayer.Start();
                    this.isVideoSourceInitialized = true;
                    this.SetVideoPlayer(true);
                }
                catch (ArgumentNullException)
                {
                    errorAction();
                }
                catch (ArgumentException)
                {
                    errorAction();
                }
            }
        }

        /// <summary>
        /// Gets video device source collection current available.
        /// </summary>
        public static IEnumerable<MediaInformation> GetVideoDevices
        {
            get
            {
                var filterVideoDeviceCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                return (from FilterInfo filterInfo in filterVideoDeviceCollection select new MediaInformation { DisplayName = filterInfo.Name, UsbId = filterInfo.MonikerString }).ToList();
            }
        }

        /// <summary>
        /// Event handler for camera video device on loaded event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="eventArgs">Event argument.</param>
        private void WebcamDeviceOnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            //// Set controls width / height based on VideoPreviewWidth / VideoPreviewHeight binding properties.
            this.NoVideoSourceGrid.Width = this.VideoPreviewWidth;
            this.VideoSourceWindowsFormsHost.Width = this.VideoPreviewWidth;
            this.NoVideoSourceGrid.Height = this.VideoPreviewHeight;
            this.VideoSourceWindowsFormsHost.Height = this.VideoPreviewHeight;
            this.InitializeVideoDevice(this.VideoSourceId);
        }

        /// <summary>
        ///  Disconnect video source device.
        /// </summary>
        private void ReleaseVideoDevice()
        {
            this.isVideoSourceInitialized = false;
            this.SetVideoPlayer(false);
            if (null == this.videoCaptureDevice)
            {
                return;
            }

            this.videoCaptureDevice.SignalToStop();
            this.videoCaptureDevice.WaitForStop();
            this.videoCaptureDevice.Stop();
            this.videoCaptureDevice = null;
        }

        /// <summary>
        /// Set video source player visibility.
        /// </summary>
        /// <param name="isVideoSourceFound">Indicates a value weather video source device found or not.</param>
        /// <param name="noVideoSourceMessage">Message to display when no video source found, optional will use empty string.</param>
        private void SetVideoPlayer(bool isVideoSourceFound, string noVideoSourceMessage = "")
        {
            //// If video source found is true show the video source player or else show no video source message.
            if (isVideoSourceFound)
            {
                this.VideoSourceWindowsFormsHost.Visibility = Visibility.Visible;
                this.NoVideoSourceGrid.Visibility = Visibility.Hidden;
                this.NoVideoSourceMessage.Text = string.Empty;
            }
            else
            {
                this.VideoSourceWindowsFormsHost.Visibility = Visibility.Hidden;
                this.NoVideoSourceGrid.Visibility = Visibility.Visible;
                this.NoVideoSourceMessage.Text = string.IsNullOrWhiteSpace(noVideoSourceMessage) ? "Камера не найдена" : noVideoSourceMessage;
            }
        }

        /// <summary>
        /// Event handler for dispatcher shutdown started event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void DispatcherShutdownStarted(object sender, EventArgs eventArgs)
        {
            this.ReleaseVideoDevice();
        }

        /// <summary>
        /// Event handler for camera video device on unloaded event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void WebcamDeviceOnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            this.ReleaseVideoDevice();
        }

    }
}
