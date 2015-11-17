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

namespace OrganizationContractsModule.ViewModels
{
    public class RecordDocumentsCollectionViewModel : BindableBase, IDisposable
    {
        private readonly IDocumentService documentService;
        private readonly ILog logService;
        private CancellationTokenSource currentLoadingToken;

        public RecordDocumentsCollectionViewModel(IDocumentService documentService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }           

            this.documentService = documentService;
            this.logService = logService;
        }

        internal async void LoadDocs(int recordId)
        {
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
                });

                RecordDocuments = new ObservableCollectionEx<RecordDocumentViewModel>(
                result.Select(x => new RecordDocumentViewModel(documentService)
                {
                    DocumentId = x.Id,
                    DocumentName = x.Name,
                    DocumentThumbnail = documentService.GetThumbnailForFile(x.Id),
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

        public void Dispose()
        {

        }

        #region Properties

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

        #endregion
    }
}
