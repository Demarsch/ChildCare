using System;
using Core;
using log4net;
using System.Windows.Navigation;
using Prism.Mvvm;
using Prism.Interactivity.InteractionRequest;
using Core.Wpf.PopupWindowActionAware;
using System.ComponentModel;
using PatientInfoModule.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Misc;
using Prism.Commands;
using System.Windows.Input;
using Core.Data;
using System.Collections.Generic;

namespace PatientInfoModule.ViewModels
{
    public class SelectPersonDocumentTypeViewModel : BindableBase, INotification, IPopupWindowActionAware, IDataErrorInfo
    {
        private IDocumentService documentService;
        private ILog log;
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; private set; }

        public SelectPersonDocumentTypeViewModel(IDocumentService documentService, ILog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            this.documentService = documentService;
            this.log = log;

            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            SelectCommand = new DelegateCommand(Select);

            DocumentTypes = new ObservableCollectionEx<OuterDocumentType>(documentService.GetOuterDocumentTypes(null));
        }

        public void IntializeCreation(string title)
        {
            this.Title = title;
        }
      
        public ICommand SelectCommand { get; private set; }
        private void Select()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                FailureMediator.Activate("Проверьте правильность заполнения полей.", null, null, true);
                return;
            }
            HostWindow.Close();
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

        #region IPopupWindowActionAware implementation
        public System.Windows.Window HostWindow { get; set; }

        public INotification HostNotification { get; set; }
        #endregion

        #region INotification implementation
        public object Content { get; set; }

        public string Title { get; set; }
        #endregion

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
    }
}
