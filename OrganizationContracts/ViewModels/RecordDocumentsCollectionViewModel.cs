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

namespace OrganizationContractsModule.ViewModels
{
    public class RecordDocumentsCollectionViewModel : BindableBase, IDisposable
    {
        private readonly IDocumentService documentService;
        private readonly IFileService fileService;
        private readonly IRecordService recordService;
        private readonly ILog logService;
        private CancellationTokenSource currentLoadingToken;

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
            detachDocumentCommand = new DelegateCommand(DetachDocument);
            attachDICOMCommand = new DelegateCommand(AttachDICOM);
            detachDICOMCommand = new DelegateCommand(DetachDICOM);
        }
        
        internal async void LoadDocs(int recordId)
        {
            SetVisibilityControlButtons(recordId);
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            IDisposableQueryable<Document> documentsQuery = null;
            
            try
            {
                documentsQuery = documentService.GetRecordDocuments(recordId);

                var result = await Task.Factory.StartNew(() =>
                {
                    return documentsQuery.Select(x => new
                    {
                        Id = x.Id,
                        Name = x.FileName,
                        DisplayName = x.DisplayName,
                        FileData = x.FileData,
                        Extension = x.Extension
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

                }
                if (documentsQuery != null)
                    documentsQuery.Dispose();
            }
        }

        private void SetVisibilityControlButtons(int recordId)
        {
            var recordType = recordService.GetRecordById(recordId).First().RecordType.EditorId;
            AllowDICOM = false;
            AllowDocuments = true;
        }

        private void AttachDocument()
        {
            string[] files = fileService.OpenFileDialog(true);
            if (files.Any())
            {

            }
        }

        private void DetachDocument()
        {
            return;
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

        private bool hasDocuments;
        public bool HasDocuments
        {
            get { return hasDocuments; }
            set { SetProperty(ref hasDocuments, value); }
        }

        private ObservableCollectionEx<RecordDocumentViewModel> recordDocuments;
        public ObservableCollectionEx<RecordDocumentViewModel> RecordDocuments
        {
            get { return recordDocuments; }
            set 
            { 
                if (SetProperty(ref recordDocuments, value) && value.Any())
                {
                    HasDocuments = true;
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
