using Core.Wpf.Mvvm;
using log4net;
using OrganizationContractsModule.Services;
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

namespace OrganizationContractsModule.ViewModels
{
    public class RecordDocumentsCollectionViewModel : BindableBase, IDisposable
    {
        private readonly IDocumentService documentService;
        private readonly IFileService fileService;
        private readonly IRecordService recordService;
        private readonly ILog logService;
        private CancellationTokenSource currentLoadingToken;
        public InteractionRequest<Confirmation> ConfirmationInteractionRequest { get; private set; }
        private int? recordId;
        private int? assignmentId;

        public RecordDocumentsCollectionViewModel(IDocumentService documentService, IRecordService recordService, IFileService fileService, ILog logService)
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
        }

        void RecordDocuments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {            
            HasAttachments = recordDocuments.Any();
            if (HasAttachments)
                recordDocuments.First().IsSelected = true;
            detachDocumentCommand.RaiseCanExecuteChanged();
        }
        
        internal async void LoadDocuments(int? assignmentId, int? recordId)
        {
            if (!assignmentId.HasValue && !recordId.HasValue) return;
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
                               
                RecordDocuments = new ObservableCollectionEx<RecordDocumentViewModel>(
                result.Select(x => new RecordDocumentViewModel(fileService, documentService)
                {
                    DocumentId = x.Id,
                    DocumentName = x.Name,
                    DocumentThumbnail = documentService.GetDocumentThumbnail(x.Id),
                    DocumentToolTip = x.DisplayName
                }).ToArray());

                RecordDocuments.CollectionChanged += RecordDocuments_CollectionChanged;

                loadingIsCompleted = true;
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
                    var recordType = (this.recordId.HasValue ? recordService.GetRecordById(this.recordId.Value).Select(x => x.RecordType).FirstOrDefault() :
                                                               recordService.GetAssignmentById(this.assignmentId.Value).Select(x => x.RecordType).FirstOrDefault());
                    if (recordType != null)
                        SetVisibilityControlButtons(recordType.Id);
            
                    detachDocumentCommand.RaiseCanExecuteChanged();
                }
                if (recordDocumentsQuery != null)
                    recordDocumentsQuery.Dispose();
            }
        }

        internal async void SetRecordToDocuments(int toRecordId)
        {
            if (!this.assignmentId.HasValue && !this.recordId.HasValue) return;
            var recordDocumentsQuery = documentService.GetRecordDocuments(this.recordId, this.assignmentId);
            await recordDocumentsQuery.ForEachAsync(x => { x.AssignmentId = (int?)null; x.RecordId = toRecordId; });
            bool isOK = await documentService.SetRecordToDocuments(recordDocumentsQuery);
        }

        private void SetVisibilityControlButtons(int recordTypeId)
        {
            var recordType = recordService.GetRecordTypeById(recordTypeId).FirstOrDefault();
            if (recordType != null && recordType.RecordTypeEditors.Any())
            {
                var editor = recordType.RecordTypeEditors.First();
                AllowDICOM = editor.HasDICOM;
                AllowDocuments = editor.HasDocuments;
                if (allowDICOM)
                    CanAttachDICOM = hasAttachments;
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
                        recordDocument.RecordId = this.recordId;
                        recordDocument.DocumentId = documentId;

                        if (recordService.SaveRecordDocument(recordDocument))
                        {
                            RecordDocuments.Add(new RecordDocumentViewModel(fileService, documentService)
                            {
                                DocumentId = documentId,
                                DocumentName = document.FileName,
                                DocumentThumbnail = documentService.GetDocumentThumbnail(documentId),
                                DocumentToolTip = document.DisplayName
                            });                            
                        }
                    }                        
                }
            }
        }

        private bool CanDetachDocuments()
        {
            if (recordDocuments != null && !recordDocuments.Any(x => x.IsSelected)) 
                return false;
            return hasAttachments;
        }

        private void DetachDocument()
        {
            ConfirmationInteractionRequest.Raise(new Confirmation()
            {
                Title = "Внимание",
                Content = "Удалить отмеченные документы ?"
            }, OnDialogClosed);
        }

        private void OnDialogClosed(Confirmation confirmation)
        {
            if (confirmation.Confirmed)
            {
                var selectedFile = recordDocuments.FirstOrDefault(x => x.IsSelected);
                if (selectedFile != null)
                {
                    recordService.DeleteRecordDocument(selectedFile.DocumentId);
                    RecordDocuments.Remove(selectedFile);
                }
            }
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

        }

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

        private bool canAttachDICOM;
        public bool CanAttachDICOM
        {
            get { return canAttachDICOM; }
            set { SetProperty(ref canAttachDICOM, value); }
        }

        private bool hasAttachments;
        public bool HasAttachments
        {
            get { return hasAttachments; }
            set { SetProperty(ref hasAttachments, value); }
        }

        private ObservableCollectionEx<RecordDocumentViewModel> recordDocuments;
        public ObservableCollectionEx<RecordDocumentViewModel> RecordDocuments
        {
            get { return recordDocuments; }
            set 
            {
                if (SetProperty(ref recordDocuments, value))
                {
                    HasAttachments = value.Any();
                    if (value.Any())
                        recordDocuments.First().IsSelected = true;
                }
            }
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
