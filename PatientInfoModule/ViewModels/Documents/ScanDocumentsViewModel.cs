using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using log4net;
using Prism.Mvvm;
using PatientInfoModule.Services;
using PatientInfoModule.ViewModels;
using Core.Data;
using Core.Wpf.Mvvm;
using System.Windows;
using System.Threading;
using Prism.Commands;
using System.Windows.Input;
using Core.Data.Misc;
using Core.Wpf.Misc;
using System.ComponentModel;
using System.Collections.Generic;
using Prism.Interactivity.InteractionRequest;

namespace PatientInfoModule.ViewModels
{
    public class ScanDocumentsViewModel : BindableBase
    {
        private ILog log;
        private IDocumentService documentService; 
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; private set; }
        private readonly CommandWrapper saveChangesCommandWrapper;
        public InteractionRequest<Notification> NotificationInteractionRequest { get; private set; }

        public ScanDocumentsViewModel(IDocumentService documentService, ILog log)
        {
            this.documentService = documentService;
            this.log = log;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            NotificationInteractionRequest = new InteractionRequest<Notification>();

            saveCommand = new DelegateCommand(Save);
            PreviewImages = new ObservableCollectionEx<ThumbnailViewModel>();
            DocumentTypes = new ObservableCollectionEx<OuterDocumentType>(documentService.GetOuterDocumentTypes(null));
            saveChangesCommandWrapper = new CommandWrapper { Command = saveCommand };
        }

       
        private readonly DelegateCommand saveCommand;
        public ICommand SaveCommand { get { return saveCommand; } }

        private async void Save()
        {
            FailureMediator.Deactivate();
            if (!AllowSave())
                return;           
            BusyMediator.Activate("Загрузка документов в БД...");
            foreach (var item in PreviewImages.Where(x => x.ThumbnailChecked))
            {
                log.InfoFormat("Upload document with Id {0}", (item.DocumentId == SpecialValues.NewId) ? "(Upload new document)" : item.DocumentId.ToString());
                var saveSuccesfull = false;
                Document document = documentService.GetDocumentById(item.DocumentId).FirstOrDefault();
                if (document == null)
                    document = new Document();
                document.FileName = documentService.GetOuterDocumentTypeById(item.DocumentTypeId).First().Name;
                document.DocumentFromDate = item.DocumentDate;
                document.Description = item.Comment;
                document.DisplayName = document.FileName + (document.DocumentFromDate.HasValue ? " от " + document.DocumentFromDate.Value.ToShortDateString() : string.Empty);
                
                document.Extension = "png";
                document.FileData = documentService.GetBinaryDataFromImage(item.ThumbnailImage);
                document.FileSize = document.FileData.Length;
                document.UploadDate = DateTime.Now;

                try
                {
                    item.DocumentId = await documentService.UploadDocument(document);
                    saveSuccesfull = true;
                }
                catch (Exception ex)
                {
                    log.Error("Failed to Upload document with Id " + ((item.DocumentId == SpecialValues.NewId) ? "(Upload new document)" : item.DocumentId.ToString()), ex);
                    FailureMediator.Activate("Не удалось загрузить документ в БД. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
                    return;
                }
                finally
                {
                    BusyMediator.Deactivate();
                    item.ThumbnailSaved = saveSuccesfull;
                }                
            }
        }

        private bool AllowSave()
        {
            if (!PreviewImages.Any() || PreviewImages.All(x => !x.ThumbnailChecked))
            {
                /*NotificationInteractionRequest.Raise(new Notification()
                {
                    Title = "Внимание",
                    Content = "Отсутствуют отсканированные документы или вы их не отметили."
                });*/
                MessageBox.Show("Отсутствуют отсканированные документы или вы их не отметили.");
                return false;
            }
            if (PreviewImages.Any(x => x.DocumentTypeId < 1))
            {
                /*NotificationInteractionRequest.Raise(new Notification()
                {
                    Title = "Внимание",
                    Content = "Один или несколько документов не имеют описания."
                });*/
                MessageBox.Show("Один или несколько документов не имеют описания.");
                return false;
            }
            if (PreviewImages.Any(x => x.NeedDate && !x.DocumentDate.HasValue))
            {
                /*NotificationInteractionRequest.Raise(new Notification()
                {
                    Title = "Внимание",
                    Content = "Один или несколько документов не имеют необходимой даты."
                });*/
                MessageBox.Show("Один или несколько документов не имеют необходимой даты.");
                return false;
            }
            return true;
        }        

        private ObservableCollectionEx<ThumbnailViewModel> previewImages;
        public ObservableCollectionEx<ThumbnailViewModel> PreviewImages
        {
            get { return previewImages; }
            set { SetProperty(ref previewImages, value); }
        }

        private ThumbnailViewModel selectedThumbnail;
        public ThumbnailViewModel SelectedThumbnail
        {
            get { return selectedThumbnail; }
            set 
            {
                if (SetProperty(ref selectedThumbnail, value) && value != null)
                {
                    CurrentScannedImage = value.ThumbnailImage;
                    if (value.DocumentTypeId != 0)
                        SelectedDocumentType = documentTypes.First(x => x.Id == value.DocumentTypeId);
                    else
                        SelectedDocumentType = null;
                    Comment = value.Comment;
                }
            }
        }

        private ObservableCollection<FieldValue> devices;
        public ObservableCollection<FieldValue> Devices
        {
            get { return devices; }
            set { SetProperty(ref devices, value); }
        }

        private FieldValue selectedDevice;
        public FieldValue SelectedDevice
        {
            get { return selectedDevice; }
            set { SetProperty(ref selectedDevice, value); }
        }

        private ObservableCollectionEx<OuterDocumentType> documentTypes;
        public ObservableCollectionEx<OuterDocumentType> DocumentTypes
        {
            get { return documentTypes; }
            set { SetProperty(ref documentTypes, value); }
        }

        private OuterDocumentType selectedDocumentType;
        public OuterDocumentType SelectedDocumentType
        {
            get { return selectedDocumentType; }
            set
            {
                if (SetProperty(ref selectedDocumentType, value) && value != null && SelectedThumbnail != null)
                {
                    SelectedThumbnail.DocumentType = value.Name;
                    SelectedThumbnail.DocumentTypeId = value.Id;
                    SelectedThumbnail.NeedDate = value.HasDate;
                    DocumentHasDate = value.HasDate;
                    if (!value.HasDate)
                        SelectedDocumentDate = null;
                }
            }
        }

        private bool documentHasDate;
        public bool DocumentHasDate
        {
            get { return documentHasDate; }
            set { SetProperty(ref documentHasDate, value); }
        }

        private string comment = String.Empty;
        public string Comment
        {
            get { return comment; }
            set
            {
                if (SetProperty(ref comment, value) && value != null && SelectedThumbnail != null)
                    SelectedThumbnail.Comment = value;
            }
        }

        private DateTime? selectedDocumentDate;
        public DateTime? SelectedDocumentDate
        {
            get { return selectedDocumentDate; }
            set
            {
                if (SetProperty(ref selectedDocumentDate, value) && SelectedThumbnail != null)
                    SelectedThumbnail.DocumentDate = value;
            }
        }

        private BitmapImage currentScannedImage;
        public BitmapImage CurrentScannedImage
        {
            get { return currentScannedImage; }
            set { SetProperty(ref currentScannedImage, value); }
        }

        private bool selectAllThumbnails;
        public bool SelectAllThumbnails
        {
            get { return selectAllThumbnails; }
            set
            {
                if (SetProperty(ref selectAllThumbnails, value))
                    PreviewImages.ToList().ForEach(x => x.ThumbnailChecked = value);
            }
        }

    }
}
