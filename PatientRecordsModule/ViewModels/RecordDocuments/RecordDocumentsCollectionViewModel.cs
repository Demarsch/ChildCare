using Core.Wpf.Mvvm;
using log4net;
using Shared.PatientRecords.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.Entity;
using Core.Expressions;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Input;
using Core.Data.Misc;
using Core.Data;
using System.Threading.Tasks;
using System.Collections;
using Prism.Commands;
using Core.Services;
using Core.Wpf.Services;
using Prism.Interactivity.InteractionRequest;
using System.Collections.Specialized;
using System.Windows;
using Core.Wpf.Misc;

namespace Shared.PatientRecords.ViewModels
{
    public class RecordDocumentsCollectionViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly IDocumentService documentService;
        private readonly IFileService fileService;
        private readonly IPatientRecordsService recordService;
        private readonly ILog logService;
        private CancellationTokenSource currentLoadingToken;
        public InteractionRequest<Confirmation> ConfirmationInteractionRequest { get; private set; }
        public InteractionRequest<Notification> NotificationInteractionRequest { get; private set; }
        private int recordId;
        private int assignmentId;
        #endregion

        #region Constructor

        public RecordDocumentsCollectionViewModel(IDocumentService documentService, IPatientRecordsService recordService, IFileService fileService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (fileService == null)
            {
                throw new ArgumentNullException("fileService");
            }  
            this.documentService = documentService;
            this.recordService = recordService;
            this.fileService = fileService;
            this.logService = logService;

            attachDocumentCommand = new DelegateCommand(AttachDocument);
            detachDocumentCommand = new DelegateCommand(DetachDocument, CanDetachDocuments);
            attachDICOMCommand = new DelegateCommand(AttachDICOM);
            detachDICOMCommand = new DelegateCommand(DetachDICOM);

            ConfirmationInteractionRequest = new InteractionRequest<Confirmation>();
            NotificationInteractionRequest = new InteractionRequest<Notification>();

            RecordDocuments = new ObservableCollectionEx<RecordDocumentViewModel>();
            RecordDocuments.BeforeCollectionChanged += OnBeforeRecordDocumentsChanged;
            RecordDocuments.CollectionChanged += OnRecordDocumentsChanged;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Load record attachments
        /// </summary>
        /// <param name="assignmentId">0 - if there is no </param>
        /// <param name="recordId">0 - if there is no </param>
        internal async void LoadDocuments(int assignmentId, int recordId)
        {
            RecordDocuments.Clear();
            if (SpecialValues.IsNewOrNonExisting(assignmentId) && SpecialValues.IsNewOrNonExisting(recordId))
            {                
                SetVisibilityControlButtons(SpecialValues.NonExistingId);
                return;
            }
            this.assignmentId = assignmentId;
            this.recordId = recordId;
            
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            IDisposableQueryable<RecordDocument> recordDocumentsQuery = null;
            
            try
            {
                recordDocumentsQuery = documentService.GetRecordDocuments(this.recordId, this.assignmentId);

                var result = await Task.Factory.StartNew(() =>
                    {
                        return recordDocumentsQuery.Select(x => new
                        {
                            Id = x.DocumentId,
                            Name = x.Document.FileName,
                            DisplayName = x.Document.DisplayName,
                            FileData = x.Document.FileData,
                            Extension = x.Document.Extension
                        }).ToArray();
                    }, token);
                
                RecordDocuments.AddRange(
                    result.Select(x => new RecordDocumentViewModel(fileService, documentService)
                    {
                        DocumentId = x.Id,
                        DocumentName = x.Name,
                        DocumentThumbnail = documentService.GetDocumentThumbnail(x.Id),
                        DocumentToolTip = x.DisplayName,
                        Extension = x.Extension
                    }).ToArray());

                loadingIsCompleted = true;
                await Task.Run(() => { Application.Current.Dispatcher.Invoke(() => detachDocumentCommand.RaiseCanExecuteChanged()); });
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                logService.Error("Failed to load org contracts", ex);
                loadingIsCompleted = false;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    var recordType = (!SpecialValues.IsNewOrNonExisting(this.recordId) ?
                                                    recordService.GetRecord(this.recordId).Select(x => x.RecordType).FirstOrDefault() :
                                                    recordService.GetAssignment(this.assignmentId).Select(x => x.RecordType).FirstOrDefault());
                    if (recordType != null)
                        SetVisibilityControlButtons(recordType.Id); 
                }
                if (recordDocumentsQuery != null)
                    recordDocumentsQuery.Dispose();
            }            
        }

        internal async void SetRecordToDocuments(int toRecordId)
        {
            if (SpecialValues.IsNewOrNonExisting(assignmentId) && SpecialValues.IsNewOrNonExisting(recordId)) return;
            var recordDocumentsQuery = documentService.GetRecordDocuments(this.recordId, this.assignmentId);
            await recordDocumentsQuery.ForEachAsync(x => { x.AssignmentId = (int?)null; x.RecordId = toRecordId; });
            bool isOK = await documentService.SetRecordToDocuments(recordDocumentsQuery);
        }

        private void SetVisibilityControlButtons(int recordTypeId)
        {            
            AllowDICOM = AllowDocuments = CanAttachDICOM = CanDetachDICOM = false;       
            var recordType = recordService.GetRecordTypeById(recordTypeId).FirstOrDefault();
            if (recordType != null && recordType.RecordTypeEditors.Any())
            {
                var editor = recordType.RecordTypeEditors.First();
                AllowDICOM = editor.HasDICOM;
                AllowDocuments = editor.HasDocuments;
                if (allowDICOM)
                {
                    CanAttachDICOM = !hasDICOMAttachments;
                    CanDetachDICOM = hasDICOMAttachments;
                }
            }        
        }    

        private async void AttachDocument()
        {
            string[] files = fileService.OpenFileDialog(true);
            if (files.Any())
            {
                int index = recordDocuments.Count(x => x.DocumentName.StartsWith("Док "));
                foreach (var file in files)
                {
                    Document document = new Document();
                    document.FileName = "Док " + (++index);
                    document.DocumentFromDate = (DateTime?)null;
                    document.Description = string.Empty;
                    document.DisplayName = file.Substring(file.LastIndexOf("\\") + 1);
                    document.Extension = file.Substring(file.LastIndexOf('.') + 1);
                    document.FileData = fileService.GetBinaryDataFromFile(file);
                    document.FileSize = document.FileData.Length;
                    document.UploadDate = DateTime.Now;

                    int documentId = await documentService.UploadDocument(document);
                    if (documentId != SpecialValues.NewId)
                    {
                        RecordDocument recordDocument = new RecordDocument();
                        recordDocument.AssignmentId = SpecialValues.IsNewOrNonExisting(this.assignmentId) ? (int?)null : this.assignmentId;
                        recordDocument.RecordId = SpecialValues.IsNewOrNonExisting(this.recordId) ? (int?)null : this.recordId;
                        recordDocument.DocumentId = documentId;

                        string exception = string.Empty;
                        if (documentService.SaveRecordDocument(recordDocument, out exception))
                        {
                            RecordDocuments.Add(new RecordDocumentViewModel(fileService, documentService)
                            {
                                DocumentId = documentId,
                                DocumentName = document.FileName,
                                DocumentThumbnail = documentService.GetDocumentThumbnail(documentId),
                                DocumentToolTip = document.DisplayName,
                                Extension = document.Extension
                            });                            
                        }
                        else
                            NotificationInteractionRequest.Raise(new Notification() { Title = "Внимание", Content = exception });
                    }                        
                }
            }
        }

        private bool CanDetachDocuments()
        {
            if (recordDocuments != null && !recordDocuments.Any(x => x.IsSelected && !x.Extension.Equals(FileServiceFilters.DICOMExtention))) 
                return false;
            return hasDocumentsAttachments;
        }

        private void DetachDocument()
        {
            ConfirmationInteractionRequest.Raise(new Confirmation()
            {
                Title = "Внимание",
                Content = "Удалить отмеченные документы ?"
            }, OnDialogClosed);
        }

        private void AttachDICOM()
        {
            return;
        }

        private void DetachDICOM()
        {
            return;
        }

        public void Dispose()
        {
            foreach (var document in RecordDocuments)
                document.PropertyChanged -= OnDocumentPropertyChanged;
            RecordDocuments.BeforeCollectionChanged -= OnBeforeRecordDocumentsChanged;
            RecordDocuments.CollectionChanged -= OnRecordDocumentsChanged;
        }

        #endregion

        #region Events

        private void OnDialogClosed(Confirmation confirmation)
        {
            if (confirmation.Confirmed)
            {
                var selectedFile = recordDocuments.FirstOrDefault(x => x.IsSelected);
                if (selectedFile != null)
                {
                    string exception = string.Empty;
                    if (documentService.DeleteRecordDocument(selectedFile.DocumentId, out exception))
                        RecordDocuments.Remove(selectedFile);
                    else
                        NotificationInteractionRequest.Raise(new Notification() { Title = "Внимание", Content = exception });
                }
            }
        }

        void OnRecordDocumentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasDocumentsAttachments = recordDocuments.Any(x => !x.Extension.Equals(FileServiceFilters.DICOMExtention));
            HasDICOMAttachments = recordDocuments.Any(x => x.Extension.Equals(FileServiceFilters.DICOMExtention));
        }

        void OnBeforeRecordDocumentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<RecordDocumentViewModel>())
                    newItem.PropertyChanged += OnDocumentPropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<RecordDocumentViewModel>())
                    oldItem.PropertyChanged -= OnDocumentPropertyChanged;
            }
        }

        private void OnDocumentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            detachDocumentCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Properties

        private bool allowDocuments;
        public bool AllowDocuments
        {
            get { return allowDocuments; }
            set { SetProperty(ref allowDocuments, value); }
        }

        private bool allowDICOM;
        public bool AllowDICOM
        {
            get { return allowDICOM; }
            set { SetProperty(ref allowDICOM, value); }
        }

        private bool canDetachDICOM;
        public bool CanDetachDICOM
        {
            get { return canDetachDICOM; }
            set { SetProperty(ref canDetachDICOM, value); }
        }

        private bool canAttachDICOM;
        public bool CanAttachDICOM
        {
            get { return canAttachDICOM; }
            set { SetProperty(ref canAttachDICOM, value); }
        }
        
        private bool hasDocumentsAttachments;
        public bool HasDocumentsAttachments
        {
            get { return hasDocumentsAttachments; }
            set 
            { 
                SetProperty(ref hasDocumentsAttachments, value);
                HasAnyAttachments = value || HasDICOMAttachments;
                OnPropertyChanged(() => HasAnyAttachments);
            }
        }

        private bool hasDICOMAttachments;
        public bool HasDICOMAttachments
        {
            get { return hasDICOMAttachments; }
            set 
            {
                SetProperty(ref hasDICOMAttachments, value);
                HasAnyAttachments = value || HasDocumentsAttachments;
                OnPropertyChanged(() => HasAnyAttachments);
            }
        }

        private bool hasAnyAttachments;
        public bool HasAnyAttachments
        {
            get { return hasAnyAttachments; }
            set { SetProperty(ref hasAnyAttachments, value); }
        }

        private ObservableCollectionEx<RecordDocumentViewModel> recordDocuments;
        public ObservableCollectionEx<RecordDocumentViewModel> RecordDocuments
        {
            get { return recordDocuments; }
            set { SetProperty(ref recordDocuments, value);}
        }

        public ICommand OpenDocumentCommand 
        { 
            get
            {
                return recordDocuments.First(x => x.IsSelected).OpenDocumentCommand;
            } 
        }

        private readonly DelegateCommand attachDocumentCommand;
        public ICommand AttachDocumentCommand { get { return attachDocumentCommand; } }

        private readonly DelegateCommand detachDocumentCommand;
        public ICommand DetachDocumentCommand { get { return detachDocumentCommand; } }

        private readonly DelegateCommand attachDICOMCommand;
        public ICommand AttachDICOMCommand { get { return attachDICOMCommand; } }

        private readonly DelegateCommand detachDICOMCommand;
        public ICommand DetachDICOMCommand { get { return detachDICOMCommand; } }

        #endregion
    }
}
