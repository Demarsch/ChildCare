using AdminTools.DTO;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminTools.ViewModel
{
    class ScanDocumentsViewModel : ObservableObject
    {
        private IScannerService scannerService;
        private IDialogService dialogService;

        public ScanDocumentsViewModel(IScannerService scannerService, IDialogService dialogService)
        {
            this.scannerService = scannerService;
            this.dialogService = dialogService;
            this.ScanCommand = new RelayCommand(Scan);
            PreviewImages = new ObservableCollection<ThumbnailDTO>();
        }

        private void Scan()
        {
            try
            {
                List<string> devices = WIAScanner.GetDevices();
                if (!devices.Any())
                {
                    dialogService.ShowError("Не обнаружено устройство сканирования. Проверьте наличие подключения.");
                    return;
                }

                List<Image> images = WIAScanner.Scan();                
                foreach (Image image in images)
                {
                    ZoomImage = image;
                    PreviewImages.Add(new ThumbnailDTO() { ImageSource = image, Description = "Описание" });
                    //image.Save(@"D:\" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".png", ImageFormat.Png);
                }
                
            }
            catch (Exception exc)
            {
                dialogService.ShowError(exc.Message);
            }
        }

        private RelayCommand scanCommand;
        public RelayCommand ScanCommand
        {
            get { return scanCommand; }
            set { Set("ScanCommand", ref scanCommand, value); }
        }

        private ObservableCollection<ThumbnailDTO> previewImages;
        public ObservableCollection<ThumbnailDTO> PreviewImages
        {
            get { return previewImages; }
            set { Set("PreviewImages", ref previewImages, value); }
        }

        private ThumbnailDTO selectedThumbnail;
        public ThumbnailDTO SelectedThumbnail
        {
            get { return selectedThumbnail; }
            set 
            {
                if (!Set("SelectedThumbnail", ref selectedThumbnail, value) || value == null)
                    return;
                ZoomImage = value.ImageSource;
            }
        }

        private Image zoomImage;
        public Image ZoomImage
        {
            get { return zoomImage; }
            set { Set("ZoomImage", ref zoomImage, value); }
        }
    }
}
