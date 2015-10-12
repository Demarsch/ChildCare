using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using log4net;

namespace MainLib.ViewModel
{
    public class ScanDocumentsViewModel : ObservableObject
    {
        private ILog log;
        private IDialogService dialogService;
        private IDocumentService documentService;

        public ScanDocumentsViewModel(IDocumentService documentService, IDialogService dialogService, ILog log)
        {
            this.dialogService = dialogService;
            this.documentService = documentService;
            this.log = log;

            this.SaveCommand = new RelayCommand(Save);
            PreviewImages = new ObservableCollection<ThumbnailDTO>();
            DocumentTypes = new ObservableCollection<OuterDocumentType>(documentService.GetOuterDocumentTypes(null));
        }

        private void Save()
        {
            if (!PreviewImages.Any() || PreviewImages.All(x => !x.ThumbnailChecked))
            {
                dialogService.ShowMessage("Отсутствуют отсканированные документы или вы их не отметили.");
                return;
            }

            foreach (var item in PreviewImages.Where(x => x.ThumbnailChecked))
            {
                string exception = string.Empty;
                Document document = documentService.GetDocumentById(item.DocumentId);
                if (document == null)
                    document = new Document();
                document.FileName = documentService.GetOuterDocumentTypeById(item.DocumentTypeId).Name;
                document.DocumentFromDate = item.DocumentDate;
                document.Description = item.Comment;
                document.DisplayName = document.FileName + (document.DocumentFromDate.HasValue ? " от " + document.DocumentFromDate.Value.ToShortDateString() : string.Empty);
                
                document.Extension = "png";
                document.FileData = documentService.GetBinaryDataFromImage(item.ThumbnailImage);
                document.FileSize = document.FileData.Length;
                document.UploadDate = DateTime.Now;               
                
                int documentId = documentService.UploadDocument(document, out exception);
                if (documentId != 0)
                {                    
                    item.DocumentId = documentId;
                    item.ThumbnailSaved = true;
                }
                else
                {
                    dialogService.ShowError("При загрузке файла в БД возникла ошибка. " + exception);
                    log.Error(string.Format("Failed to upload document to database. " + exception));
                }
                
            }
        }        

        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get { return saveCommand; }
            set { Set("SaveCommand", ref saveCommand, value); }
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
                CurrentScannedImage = value.ThumbnailImage;
                if (value.DocumentTypeId != 0)
                    SelectedDocumentType = documentTypes.First(x => x.Id == value.DocumentTypeId);
                else
                    SelectedDocumentType = null;
                Comment = value.Comment;
            }
        }

        private ObservableCollection<OuterDocumentType> documentTypes;
        public ObservableCollection<OuterDocumentType> DocumentTypes
        {
            get { return documentTypes; }
            set { Set("DocumentTypes", ref documentTypes, value); }
        }

        private OuterDocumentType selectedDocumentType;
        public OuterDocumentType SelectedDocumentType
        {
            get { return selectedDocumentType; }
            set
            {
                if (!Set("SelectedDocumentType", ref selectedDocumentType, value) || value == null || SelectedThumbnail == null)
                    return;
                SelectedThumbnail.DocumentType = value.Name;
                SelectedThumbnail.DocumentTypeId = value.Id;
                DocumentHasDate = value.HasDate;
                if (!value.HasDate)
                    SelectedDocumentDate = null;
            }
        }

        private bool documentHasDate;
        public bool DocumentHasDate
        {
            get { return documentHasDate; }
            set { Set("DocumentHasDate", ref documentHasDate, value); }
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

        private DateTime? selectedDocumentDate;
        public DateTime? SelectedDocumentDate
        {
            get { return selectedDocumentDate; }
            set
            {
                if (!Set("SelectedDocumentDate", ref selectedDocumentDate, value) || SelectedThumbnail == null)
                    return;
                SelectedThumbnail.DocumentDate = value;
            }
        }

        private BitmapImage currentScannedImage;
        public BitmapImage CurrentScannedImage
        {
            get { return currentScannedImage; }
            set { Set("CurrentScannedImage", ref currentScannedImage, value); }
        }       
    }
}
