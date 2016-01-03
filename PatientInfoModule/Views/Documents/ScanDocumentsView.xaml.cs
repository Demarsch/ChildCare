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
using PatientInfoModule.ViewModels;
using Core.Wpf.Mvvm;
using System.Collections.ObjectModel;

namespace PatientInfoModule.Views
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
                var context = (this.DataContext as ScanDocumentsViewModel);
                try
                {
                    var hook = new WPFWindowMessageHook(this);
                    twain = new Twain(hook);
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
                        context.PreviewImages.Add(
                            new ThumbnailViewModel()
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
                        context.SelectedThumbnail = context.PreviewImages.LastOrDefault();
                    }
                };

                var scannerSource = twain.SourceNames.Where(x => !x.Contains("WIA")).ToArray();
                if (scannerSource != null && scannerSource.Any())
                {
                    context.Devices = new ObservableCollection<FieldValue>();
                    for (int i = 0; i < scannerSource.Count(); i++)
                        context.Devices.Add(new FieldValue() { Value = i, Field = scannerSource[i].ToString()});
                    context.SelectedDevice = context.Devices.FirstOrDefault();
                }
                else
                    ScanButton.IsEnabled = false;     
                
                //ScannerSources.ItemsSource = scannerSource;
                //if (scannerSource != null && scannerSource.Count > 0)
                //    ScannerSources.SelectedItem = scannerSource[0];
                //else
                //    ScanButton.IsEnabled = false;                    
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

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            scannerSettings = new ScanSettings
            {
                UseDocumentFeeder = false,
                ShowTwainUI = false,
                ShowProgressIndicatorUI = true,
                UseDuplex = UseDuplex.IsChecked,
                ShouldTransferAllPages = true,
                Resolution = ResolutionSettings.ColourPhotocopier
            };

            try
            {
                twain.SelectSource((this.DataContext as ScanDocumentsViewModel).SelectedDevice.Field);
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
