using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using TwainDotNet;
using TwainDotNet.Wpf;
using TwainDotNet.Win32;
using System.Windows.Interop;
using System.IO;
using Core;

namespace MainLib.View
{
    /// <summary>
    /// Interaction logic for ScanDocumentsView.xaml
    /// </summary>
    public partial class ScanDocumentsView : Window
    {
        //public static readonly DependencyProperty ThumbnailImageProperty = DependencyProperty.Register("CurrentScannedImage", typeof(Image), typeof(ScanDocumentsView), new PropertyMetadata(ScanPropertyChangedCallback));
        private Twain twain;
        private ScanSettings scannerSettings;

        public ScanDocumentsView()
        {
            InitializeComponent();

            Loaded += delegate
            {
                try
                {
                    twain = new Twain(new WpfWindowMessageHook(this));
                }
                catch
                {
                    MessageBox.Show("Cannot connect to scanner device");
                    return;
                }
                twain.TransferImage += delegate(Object sender, TransferImageEventArgs args)
                {
                    if (args.Image != null)
                    {
                        IntPtr hbitmap = new Bitmap(args.Image).GetHbitmap();
                        (this.DataContext as MainLib.ViewModel.ScanDocumentsViewModel).PreviewImages.Add(
                            new ThumbnailDTO()
                            {
                                ThumbnailImage =
                                   BitmapFromSource(Imaging.CreateBitmapSourceFromHBitmap(
                                   hbitmap,
                                   IntPtr.Zero,
                                   Int32Rect.Empty,
                                   BitmapSizeOptions.FromEmptyOptions())),
                                ThumbnailChecked = true,
                                ThumbnailSaved = false,
                                DocumentType = "Неизвестно",
                                DocumentDate = (DateTime?)null
                            });
                        Gdi32Native.DeleteObject(hbitmap);
                        (this.DataContext as MainLib.ViewModel.ScanDocumentsViewModel).SelectedThumbnail = (this.DataContext as MainLib.ViewModel.ScanDocumentsViewModel).PreviewImages.LastOrDefault();
                    }
                };

                var scannerSource = twain.SourceNames;
                ScannerSources.ItemsSource = scannerSource;

                if (scannerSource != null && scannerSource.Count > 0)
                    ScannerSources.SelectedItem = scannerSource[0];
                else
                    ScanButton.IsEnabled = false;                    
            };            
        }

        private BitmapImage BitmapFromSource(BitmapSource bitmapsource)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
            encoder.Save(memoryStream);

            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        //public Image CurrentScannedImage
        //{
        //    get { return (Image)GetValue(ThumbnailImageProperty); }
        //    set { SetValue(ThumbnailImageProperty, value); }
        //}

        //private static void ScanPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var scanDocumentView = d as ScanDocumentsView;
        //}

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            scannerSettings = new ScanSettings
            {
                UseDocumentFeeder = false,
                ShowTwainUI = false,
                ShowProgressIndicatorUI = true,
                UseDuplex = UseDuplex.IsChecked,
                ShouldTransferAllPages = true,
                Resolution = ResolutionSettings.Photocopier
            };

            try
            {
                twain.SelectSource(ScannerSources.SelectedItem.ToString());
            }
            catch
            {
                MessageBox.Show("Cannot connect to scanner device");
                return;
            }
            try
            {
                twain.StartScanning(scannerSettings);
            }
            catch (TwainException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
