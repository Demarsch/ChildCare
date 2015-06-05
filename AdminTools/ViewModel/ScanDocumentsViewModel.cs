using AdminTools.DTO;
using Core;
using DataLib;
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
        private IPersonService personService;

        public ScanDocumentsViewModel(IScannerService scannerService, IDialogService dialogService, IPersonService personService)
        {
            this.scannerService = scannerService;
            this.dialogService = dialogService;
            this.personService = personService;

            this.ScanCommand = new RelayCommand(Scan);
            PreviewImages = new ObservableCollection<ThumbnailDTO>();
            DocumentTypes = new ObservableCollection<IdentityDocumentType>(personService.GetIdentityDocumentTypes());
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

                SelectedDocumentType = null;
                SelectedThumbnail = null;
                Comment = string.Empty;
                ZoomImage = null;

                List<Image> images = WIAScanner.Scan(devices.First());
                bool isFirstScannedImage = true;
                foreach (Image image in images)
                {
                    ZoomImage = image;
                    PreviewImages.Add(new ThumbnailDTO() { ImageSource = image, ThumbnailChecked = true, DocumentType = "Неизвестно" });
                    if (isFirstScannedImage)
                        SelectedThumbnail = PreviewImages.Last();
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
                if (value.DocumentTypeId != 0)
                    SelectedDocumentType = documentTypes.First(x => x.Id == value.DocumentTypeId);
                else
                    SelectedDocumentType = null;
                Comment = value.Comment;
            }
        }

        private ObservableCollection<IdentityDocumentType> documentTypes;
        public ObservableCollection<IdentityDocumentType> DocumentTypes
        {
            get { return documentTypes; }
            set { Set("DocumentTypes", ref documentTypes, value); }
        }

        private IdentityDocumentType selectedDocumentType;
        public IdentityDocumentType SelectedDocumentType
        {
            get { return selectedDocumentType; }
            set
            {
                if (!Set("SelectedDocumentType", ref selectedDocumentType, value) || value == null || SelectedThumbnail == null)
                    return;
                SelectedThumbnail.DocumentType = value.Name;
                SelectedThumbnail.DocumentTypeId = value.Id;
            }
        }

        private string comment = String.Empty;
        public string Comment
        {
            get { return comment; }
            set
            {
                if (!Set("Comment", ref comment, value) || value == null || SelectedThumbnail == null)
                    return;
                SelectedThumbnail.Comment = value;
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
