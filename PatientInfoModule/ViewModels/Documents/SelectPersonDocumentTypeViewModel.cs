using System;
using log4net;
using Prism.Mvvm;
using Prism.Interactivity.InteractionRequest;
using Core.Wpf.PopupWindowActionAware;
using System.ComponentModel;
using PatientInfoModule.Services;
using Core.Wpf.Mvvm;
using Prism.Commands;
using System.Windows.Input;
using Core.Data;
using System.Collections.Generic;
using System.Windows.Navigation;
using Core.Wpf.Services;
using Shared.Patient.Services;

namespace PatientInfoModule.ViewModels
{
    public class SelectPersonDocumentTypeViewModel : BindableBase, IDataErrorInfo, IDialogViewModel
    {
        private IDocumentService documentService;
        private ILog log;
        private IDialogService messageService;

        public SelectPersonDocumentTypeViewModel(IDocumentService documentService, IDialogService messageService, ILog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            this.documentService = documentService;
            this.messageService = messageService;
            this.log = log;

            CloseCommand = new DelegateCommand<bool?>(Close);
        }

        public void Initialize()
        {
            DocumentTypes = new ObservableCollectionEx<OuterDocumentType>(documentService.GetOuterDocumentTypes(null));
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
                if (SetProperty(ref selectedDocumentType, value) && value != null)
                {
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

        private DateTime? selectedDocumentDate;
        public DateTime? SelectedDocumentDate
        {
            get { return selectedDocumentDate; }
            set { SetProperty(ref selectedDocumentDate, value); }
        }

        private string description = String.Empty;
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }           

        #region IDataErrorInfo implementation
        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                switch (columnName)
                {
                    case "SelectedDocumentType":
                        result = SelectedDocumentType == null ? "Укажите тип документа" : string.Empty;
                        break;
                    case "SelectedDocumentDate":
                        result = !SelectedDocumentDate.HasValue && DocumentHasDate ? "Укажите дату документа" : string.Empty;
                        break;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region IDialogViewModel

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

        public DelegateCommand<bool?> CloseCommand { get; private set; }
        public bool TypeWasSelected = false;

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                if (IsValid)
                {
                    TypeWasSelected = true;
                    OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
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
