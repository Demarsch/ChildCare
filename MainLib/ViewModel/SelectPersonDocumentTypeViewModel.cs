using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using DataLib;
using Core;
using log4net;
using GalaSoft.MvvmLight;
using System.Windows.Navigation;

namespace MainLib.ViewModel
{
    public class SelectPersonDocumentTypeViewModel : ObservableObject, IDialogViewModel
    {
        private IDialogService dialogService;
        private IDocumentService documentService;
        private ILog log;
               
        public SelectPersonDocumentTypeViewModel(IDocumentService documentService, IDialogService dialogService, ILog log)
        {
            this.documentService = documentService;
            this.dialogService = dialogService;
            this.log = log;

            this.CloseCommand = new RelayCommand<object>(x => Close((bool?)x));

            DocumentTypes = new ObservableCollection<OuterDocumentType>(documentService.GetOuterDocumentTypes(null));
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
                if (!Set("SelectedDocumentType", ref selectedDocumentType, value) || value == null)
                    return;
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

        private DateTime? selectedDocumentDate;
        public DateTime? SelectedDocumentDate
        {
            get { return selectedDocumentDate; }
            set { Set("SelectedDocumentDate", ref selectedDocumentDate, value); }
        }

        private string description = String.Empty;
        public string Description
        {
            get { return description; }
            set { Set("Description", ref description, value); }
        }

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Тип документа"; }
        }

        public string ConfirmButtonText
        {
            get { return "Выбрать"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }
        
        public RelayCommand<object> CloseCommand { get; set; }

        private void Close(bool? validate)
        {
            if (validate == true)
            {
                if (SelectedDocumentType == null)
                {
                    dialogService.ShowMessage("Не выбран тип документа");
                    return;
                }
                OnCloseRequested(new ReturnEventArgs<bool>(true));
            }
            else 
                OnCloseRequested(new ReturnEventArgs<bool>(false));
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}
