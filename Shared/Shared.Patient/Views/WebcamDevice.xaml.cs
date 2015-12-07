using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AForge.Video.DirectShow;
using Prism.Commands;
using Shared.Patient.ViewModels;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Shared.Patient.Views
{
    /// <summary>
    /// Interaction logic for Photoes.xaml
    /// </summary>
    public partial class WebcamDevice
    {
        public static readonly DependencyProperty VideoPreviewWidthProperty = DependencyProperty.Register("VideoPreviewWidth", typeof(int), typeof(WebcamDevice), new PropertyMetadata(VideoPreviewWidthPropertyChangedCallback));
        public static readonly DependencyProperty VideoPreviewHeightProperty = DependencyProperty.Register("VideoPreviewHeight", typeof(int), typeof(WebcamDevice), new PropertyMetadata(VideoPreviewHeightPropertyChangedCallback));
        public static readonly DependencyProperty VideoSourceIdProperty = DependencyProperty.Register("VideoSourceId", typeof(string), typeof(WebcamDevice),
            new PropertyMetadata(string.Empty, VideoSourceIdPropertyChangedCallback, VideoSourceIdPropertyCoherceValueChanged));
        public static readonly DependencyProperty SnapshotBitmapProperty = DependencyProperty.Register("SnapshotBitmap", typeof(Bitmap), typeof(WebcamDevice), new PropertyMetadata(SnapshotBitmapPropertyChangedCallback));
        public static readonly DependencyProperty TakeSnapshotCommandProperty = DependencyProperty.RegisterAttached("TakeSnapshotCommandCommand", typeof(ICommand), typeof(WebcamDevice), new PropertyMetadata(null));
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
            Dispatcher.ShutdownStarted += DispatcherShutdownStarted;
            //// Initialize take snapshot command.
            TakeSnapshotCommand = new DelegateCommand(TakeSnapshotCallback);
        }

        private void TakeSnapshotCallback()
        {
            try
            {
                var playerPoint = new Point();
                //// Get the position of the source video device player.
                if (string.IsNullOrWhiteSpace(VideoSourceId))
                {
                    var noVideoDeviceSourcePoint = noVideoSourceGrid.PointToScreen(new System.Windows.Point(0, 0));
                    playerPoint.X = (int)noVideoDeviceSourcePoint.X;
                    playerPoint.Y = (int)noVideoDeviceSourcePoint.Y;
                }
                else
                {
                    playerPoint = videoSourcePlayer.PointToScreen(new Point(videoSourcePlayer.ClientRectangle.X, videoSourcePlayer.ClientRectangle.Y));
                }
                if (double.IsNaN(VideoPreviewWidth) || double.IsNaN(VideoPreviewHeight))
                {
                    using (var bitmap = new Bitmap((int)videoSourceWindowsFormsHost.ActualWidth, (int)videoSourceWindowsFormsHost.ActualHeight))
                    {
                        using (var graphicsFromImage = Graphics.FromImage(bitmap))
                        {
                            graphicsFromImage.CopyFromScreen(playerPoint, Point.Empty, new Size((int)videoSourceWindowsFormsHost.ActualWidth, (int)videoSourceWindowsFormsHost.ActualHeight));
                        }
                        SnapshotBitmap = new Bitmap(bitmap);
                    }
                }
                else
                {
                    using (var bitmap = new Bitmap((int)VideoPreviewWidth, (int)VideoPreviewHeight))
                    {
                        using (var graphicsFromImage = Graphics.FromImage(bitmap))
                        {
                            graphicsFromImage.CopyFromScreen(playerPoint, Point.Empty, new Size((int)VideoPreviewWidth, (int)VideoPreviewHeight));
                        }
                        SnapshotBitmap = new Bitmap(bitmap);
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
            var webcamDeviceView = (WebcamDevice)d;
            webcamDeviceView.Width = (int)e.NewValue;
            webcamDeviceView.videoSourcePlayer.Width = (int)e.NewValue;
        }

        public int VideoPreviewHeight
        {
            get { return (int)GetValue(VideoPreviewHeightProperty); }
            set { SetValue(VideoPreviewHeightProperty, value); }
        }

        private static void VideoPreviewHeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webcamDeviceView = (WebcamDevice)d;
            webcamDeviceView.Height = (int)e.NewValue;
            webcamDeviceView.videoSourcePlayer.Width = (int)e.NewValue;
        }

        public string VideoSourceId
        {
            get { return (string)GetValue(VideoSourceIdProperty); }
            set { SetValue(VideoSourceIdProperty, value); }
        }

        /// <summary>
        /// Gets or sets take snapshot command.
        /// </summary>
        public ICommand TakeSnapshotCommand
        {
            get { return (ICommand)GetValue(TakeSnapshotCommandProperty); }
            set { SetValue(TakeSnapshotCommandProperty, value); }
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
            if (isVideoSourceInitialized)
            {
                return;
            }
            var errorAction = new Action(() => SetVideoPlayer(false, "Невозможно подключиться к видео устройсту"));
            ReleaseVideoDevice();
            if (string.IsNullOrEmpty(videoDeviceSourceId))
            {
                return;
            }
            if (videoDeviceSourceId.StartsWith("Message:", StringComparison.OrdinalIgnoreCase))
            {
                var splitString = videoDeviceSourceId.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitString.Length == 2)
                {
                    SetVideoPlayer(false, splitString[1]);
                }
                else
                {
                    SetVideoPlayer(false);
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
                    videoCaptureDevice = new VideoCaptureDevice(videoDeviceSourceId);
                    videoSourcePlayer.VideoSource = videoCaptureDevice;
                    videoSourcePlayer.Start();
                    isVideoSourceInitialized = true;
                    SetVideoPlayer(true);
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
            noVideoSourceGrid.Width = VideoPreviewWidth;
            videoSourceWindowsFormsHost.Width = VideoPreviewWidth;
            noVideoSourceGrid.Height = VideoPreviewHeight;
            videoSourceWindowsFormsHost.Height = VideoPreviewHeight;
            InitializeVideoDevice(VideoSourceId);
        }

        /// <summary>
        ///  Disconnect video source device.
        /// </summary>
        private void ReleaseVideoDevice()
        {
            isVideoSourceInitialized = false;
            SetVideoPlayer(false);
            if (null == videoCaptureDevice)
            {
                return;
            }

            videoCaptureDevice.SignalToStop();
            videoCaptureDevice.WaitForStop();
            videoCaptureDevice.Stop();
            videoCaptureDevice = null;
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
                videoSourceWindowsFormsHost.Visibility = Visibility.Visible;
                noVideoSourceGrid.Visibility = Visibility.Hidden;
                this.noVideoSourceMessage.Text = string.Empty;
            }
            else
            {
                videoSourceWindowsFormsHost.Visibility = Visibility.Hidden;
                noVideoSourceGrid.Visibility = Visibility.Visible;
                this.noVideoSourceMessage.Text = string.IsNullOrWhiteSpace(noVideoSourceMessage) ? "Камера не найдена" : noVideoSourceMessage;
            }
        }

        /// <summary>
        /// Event handler for dispatcher shutdown started event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void DispatcherShutdownStarted(object sender, EventArgs eventArgs)
        {
            ReleaseVideoDevice();
        }

        /// <summary>
        /// Event handler for camera video device on unloaded event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void WebcamDeviceOnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            ReleaseVideoDevice();
        }

    }
}
