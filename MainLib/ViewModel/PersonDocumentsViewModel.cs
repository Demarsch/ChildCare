using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using MainLib.View;

namespace MainLib.ViewModel
{
    public class PersonDocumentsViewModel : ObservableObject
    {
        private ILog log;
        private IDialogService dialogService;
        private IDocumentService documentService;
        private IPersonService personService;
        private int personId;
        public List<KeyValuePair<int, int>> savedDocuments;

        public PersonDocumentsViewModel(IPersonService personService, IDocumentService documentService, IDialogService dialogService, ILog log)
        {            
            this.personService = personService;
            this.dialogService = dialogService;
            this.documentService = documentService;
            this.log = log;

            this.ScanDocumentCommand = new RelayCommand(ScanDocument);
            this.RemoveDocumentCommand = new RelayCommand(RemoveDocument);
            this.AttachDocumentCommand = new RelayCommand(AttachDocument);
            this.OpenFileCommand = new RelayCommand(OpenFile);            
        }

        public async void Load(int personId)
        {
            this.personId = personId;
            PatientFIO = personService.GetPersonById(this.personId).FullName;
            AllowDocumentsAction = true;
            await LoadDocuments();                        
        }

        private async Task LoadDocuments()
        {            
            AllDocuments = new ObservableCollection<ThumbnailDTO>();
            foreach (var personDocument in personService.GetPersonOuterDocuments(this.personId))
            {
                var doc = documentService.GetDocumentById(personDocument.DocumentId);
                AllDocuments.Add(new ThumbnailDTO()
                {
                    DocumentId = personDocument.DocumentId,
                    DocumentTypeId = personDocument.OuterDocumentTypeId,
                    DocumentType = doc.FileName,
                    DocumentTypeParentName = documentService.GetParentOuterDocumentTypeById(personDocument.OuterDocumentTypeId).Name,
                    Comment = doc.Description,
                    DocumentDate = doc.DocumentFromDate,
                    ThumbnailImage = documentService.GetThumbnailForFile(doc.FileData, doc.Extension),
                    ThumbnailChecked = false
                });
                await Task.Delay(10);    
            }
        }

        private void RemoveDocument()
        {
            if (!allDocuments.Any() || allDocuments.All(x => !x.ThumbnailChecked))
            {
                this.dialogService.ShowMessage("Отсутствуют документы для удаления");
                return;
            }

            if (this.dialogService.AskUser("Удалить отмеченные документы ?", true) == true)
            {
                foreach (var item in allDocuments.Where(x => x.ThumbnailChecked).ToList())
                {
                    personService.DeletePersonOuterDocument(item.DocumentId);
                    documentService.DeleteDocumentById(item.DocumentId);
                    allDocuments.Remove(item);
                }
            }
        }

        private void ScanDocument()
        {
            var scanDocumentViewModel = new ScanDocumentsViewModel(this.documentService, this.dialogService, this.log);
            (new ScanDocumentsView() { DataContext = scanDocumentViewModel }).ShowDialog();

            foreach (var item in scanDocumentViewModel.PreviewImages.Where(x => x.ThumbnailSaved))
            {
                var doc = documentService.GetDocumentById(item.DocumentId);

                PersonOuterDocument personOuterDocument = new PersonOuterDocument();
                personOuterDocument.PersonId = this.personId;
                personOuterDocument.DocumentId = item.DocumentId;
                personOuterDocument.OuterDocumentTypeId = item.DocumentTypeId;

                string exception = string.Empty;
                if (!personService.SavePersonDocument(personOuterDocument, out exception))
                {
                    dialogService.ShowError("При сохранении документа (" + doc.FileName + ") возникла ошибка. " + exception);
                    log.Error(string.Format("Failed to save patient documents. " + exception));
                }

                AllDocuments.Add(new ThumbnailDTO()
                {
                    DocumentId = item.DocumentId,
                    DocumentTypeId = item.DocumentTypeId,
                    DocumentType = doc.FileName,
                    DocumentTypeParentName = documentService.GetParentOuterDocumentTypeById(item.DocumentTypeId).Name,
                    Comment = doc.Description,
                    DocumentDate = doc.DocumentFromDate,
                    ThumbnailImage = documentService.GetThumbnailForFile(doc.FileData, doc.Extension),
                    ThumbnailChecked = false
                });
            }
        }

        private void AttachDocument()
        {      
            string[] files = this.dialogService.ShowOpenFileDialog(false);
            if (files.Any())
            {                
                var selectPersonDocumentTypeViewModel = new SelectPersonDocumentTypeViewModel(documentService, dialogService, log);
                var dialogResult = dialogService.ShowDialog(selectPersonDocumentTypeViewModel);
                if (dialogResult == true)
                {
                    string exception = string.Empty;
                    int documentTypeId = selectPersonDocumentTypeViewModel.SelectedDocumentType.Id;                    
                    Document document = new Document();
                    document.FileName = documentService.GetOuterDocumentTypeById(documentTypeId).Name;
                    document.DocumentFromDate = selectPersonDocumentTypeViewModel.SelectedDocumentDate;
                    document.Description = selectPersonDocumentTypeViewModel.Description;
                    document.DisplayName = document.FileName + (document.DocumentFromDate.HasValue ? " от " + document.DocumentFromDate.Value.ToShortDateString() : string.Empty);
                    
                    document.Extension = files[0].Substring(files[0].LastIndexOf('.') + 1);
                    document.FileData = documentService.GetBinaryDataFromFile(files[0]);
                    document.FileSize = document.FileData.Length;
                    document.UploadDate = DateTime.Now;

                    int documentId = documentService.UploadDocument(document, out exception);
                    if (documentId != 0)
                    {
                        PersonOuterDocument personOuterDocument = new PersonOuterDocument();
                        personOuterDocument.PersonId = this.personId;
                        personOuterDocument.DocumentId = documentId;
                        personOuterDocument.OuterDocumentTypeId = documentTypeId;

                        if (!personService.SavePersonDocument(personOuterDocument, out exception))
                        {
                            dialogService.ShowError("При сохранении документа (" + document.FileName + ") возникла ошибка. " + exception);
                            log.Error(string.Format("Failed to save patient documents. " + exception));
                        }
                    }
                    else
                    {
                        dialogService.ShowError("При загрузке файла в БД возникла ошибка. " + exception);
                        log.Error(string.Format("Failed to upload document to database. " + exception));
                    }

                    AllDocuments.Add(new ThumbnailDTO()
                    {
                        DocumentId = documentId,
                        DocumentTypeId = documentTypeId,
                        DocumentType = document.FileName,
                        DocumentTypeParentName = documentService.GetParentOuterDocumentTypeById(documentTypeId).Name,
                        Comment = document.Description,
                        DocumentDate = document.DocumentFromDate,
                        ThumbnailImage = documentService.GetThumbnailForFile(document.FileData, document.Extension),
                        ThumbnailChecked = false
                    });
                }
            }
        }

        private void OpenFile()
        {
            var doc = documentService.GetDocumentById(SelectedDocument.DocumentId);
            documentService.RunFile(documentService.GetFileFromBinaryData(doc.FileData, doc.Extension));
        }

        private RelayCommand scanDocumentCommand;
        public RelayCommand ScanDocumentCommand
        {
            get { return scanDocumentCommand; }
            set { Set("ScanDocumentCommand", ref scanDocumentCommand, value); }
        }

        private RelayCommand removeDocumentCommand;
        public RelayCommand RemoveDocumentCommand
        {
            get { return removeDocumentCommand; }
            set { Set("RemoveDocumentCommand", ref removeDocumentCommand, value); }
        }

        private RelayCommand attachDocumentCommand;
        public RelayCommand AttachDocumentCommand
        {
            get { return attachDocumentCommand; }
            set { Set("AttachDocumentCommand", ref attachDocumentCommand, value); }
        }

        private RelayCommand openFileCommand;
        public RelayCommand OpenFileCommand
        {
            get { return openFileCommand; }
            set { Set("OpenFileCommand", ref openFileCommand, value); }
        }

        private ObservableCollection<ThumbnailDTO> allDocuments;
        public ObservableCollection<ThumbnailDTO> AllDocuments
        {
            get { return allDocuments; }
            set { Set("AllDocuments", ref allDocuments, value); }
        }

        private ThumbnailDTO selectedDocument;
        public ThumbnailDTO SelectedDocument
        {
            get { return selectedDocument; }
            set { Set("SelectedDocument", ref selectedDocument, value); }
        }        

        private string patientFIO;
        public string PatientFIO
        {
            get { return patientFIO; }
            set { Set("PatientFIO", ref patientFIO, value); }
        }

        private bool allowDocumentsAction;
        public bool AllowDocumentsAction
        {
            get { return allowDocumentsAction; }
            set { Set("AllowDocumentsAction", ref allowDocumentsAction, value); }
        }
    }
}
