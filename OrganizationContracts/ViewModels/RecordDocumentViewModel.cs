using log4net;
using OrganizationContractsModule.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OrganizationContractsModule.ViewModels
{
    public class RecordDocumentViewModel : BindableBase
    {
        private readonly DelegateCommand openDocumentCommand;
        private readonly IDocumentService documentService;

        public RecordDocumentViewModel(IDocumentService documentService)
        {
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            this.documentService = documentService;
            openDocumentCommand = new DelegateCommand(OpenDocument);
        }

        public ICommand OpenDocumentCommand { get { return openDocumentCommand; } }
        private void OpenDocument()
        {
            documentService.RunFile(documentService.GetFileFromBinaryDocumentData(documentId));
        }

        #region Properties

        private int documentId;
        public int DocumentId
        {
            get { return documentId; }
            set { SetProperty(ref documentId, value); }
        }

        private BitmapImage documentThumbnail;
        public BitmapImage DocumentThumbnail
        {
            get { return documentThumbnail; }
            set { SetProperty(ref documentThumbnail, value); }
        }       

        private string documentName;
        public string DocumentName
        {
            get { return documentName; }
            set { SetProperty(ref documentName, value); }
        }

        private string documentToolTip;
        public string DocumentToolTip
        {
            get { return documentToolTip; }
            set { SetProperty(ref documentToolTip, value); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set 
            {
                SetProperty(ref isSelected, value);
            }
        }
               
        #endregion
    }
}
