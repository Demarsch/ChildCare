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
            RecordDocuments = new ObservableCollectionEx<RecordDocumentViewModel>();
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
            try
            {
                var result = await documentService.GetRecordDocuments(recordId)
                                .Select(x => new
                                {
                                    Id = x.Id,
                                    Name = x.FileName,
                                    DisplayName = x.DisplayName,
                                    FileData = x.FileData,
                                    Extension = x.Extension
                                }).ToArrayAsync(token);

                RecordDocuments.Clear();
                RecordDocuments.AddRange(
                    result.Select(x => new RecordDocumentViewModel()
                    {
                        DocumentId = x.Id,
                        DocumentName = x.Name,
                        DocumentThumbnail = documentService.GetThumbnailForFile(x.FileData, x.Extension),
                        DocumentToolTip = x.DisplayName 
                    }).ToArray());
                if (recordDocuments.Any())
                    recordDocuments.First().IsSelected = true;
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
            }
        }

        public void Dispose()
        {

        }

        #region Properties

        private ObservableCollectionEx<RecordDocumentViewModel> recordDocuments;
        public ObservableCollectionEx<RecordDocumentViewModel> RecordDocuments
        {
            get { return recordDocuments; }
            set { SetProperty(ref recordDocuments, value); }
        }

        public ICommand OpenDocumentCommand { get { return RecordDocuments.First(x => x.IsSelected).OpenDocumentCommand; } }

        #endregion
    }
}
