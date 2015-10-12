using DataLib;
using System;
using GalaSoft.MvvmLight;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Linq;
using log4net;
using System.Windows.Navigation;
using System.Collections.Generic;

namespace Registry
{
    public class PersonInsuranceDocumentsViewModel : ObservableObject, IDialogViewModel
    {

        #region Fields

        private int personId;

        private readonly ILog log;

        private readonly IPersonService service;

        private readonly IDialogService dialogService;

        #endregion

        #region Constructors

        public PersonInsuranceDocumentsViewModel(int personId, ILog log, IPersonService service, IDialogService dialogService)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.log = log;
            this.service = service;
            this.personId = personId;
            ListInsuranceDocumentTypes = new ObservableCollection<InsuranceDocumentType>(service.GetInsuranceDocumentTypes());
            InsuranceDocuments = new ObservableCollection<InsuranceDocumentViewModel>(service.GetInsuranceDocuments(this.personId).Select(x => new InsuranceDocumentViewModel(x, service)));
            AddInsuranceDocumentCommand = new RelayCommand(AddInsuranceDocument);
            DeleteInsuranceDocumentCommand = new RelayCommand<InsuranceDocumentViewModel>(DeleteInsuranceDocument);
            InsuranceCompanySuggestionProvider = new InsuranceCompanySuggestionProvider(service);
            CloseCommand = new RelayCommand<object>(x => Close((bool?)x));
        }

        #endregion

        #region Properties

        private InsuranceCompanySuggestionProvider insuranceCompanySuggestionProvider;
        public InsuranceCompanySuggestionProvider InsuranceCompanySuggestionProvider
        {
            get { return insuranceCompanySuggestionProvider; }
            set { Set("InsuranceCompanySuggestionProvider", ref insuranceCompanySuggestionProvider, value); }
        }

        private ObservableCollection<InsuranceDocumentViewModel> insuranceDocuments = new ObservableCollection<InsuranceDocumentViewModel>();
        public ObservableCollection<InsuranceDocumentViewModel> InsuranceDocuments
        {
            get { return insuranceDocuments; }
            set
            {
                Set("InsuranceDocuments", ref insuranceDocuments, value);
                RaisePropertyChanged("PersonInsuranceDocumentsHasNoItems");
            }
        }

        public bool PersonInsuranceDocumentsHasNoItems
        {
            get { return InsuranceDocuments == null || InsuranceDocuments.Count < 1; }
        }

        public string ActialInsuranceDocumentsString
        {
            get
            {
                string resStr = string.Empty;
                var dateTimeNow = DateTime.Now;
                foreach (var insuranceDocument in InsuranceDocuments.Where(x => dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate))
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    var insuranceDocumentType = service.GetInsuranceDocumentTypeById(insuranceDocument.InsuranceDocumentTypeId);
                    var insuranceDocumentTypeName = string.Empty;
                    if (insuranceDocumentType != null)
                        insuranceDocumentTypeName = insuranceDocumentType.Name;
                    resStr += String.Format("Тип документа: {0}\r\nСтраховая организация: {1}\r\nСерия {2} Номер {3}; Период действия {4}-{5}",
                         insuranceDocumentTypeName, (insuranceDocument.InsuranceCompany != null ? insuranceDocument.InsuranceCompany.NameSMOK : string.Empty), insuranceDocument.Series, insuranceDocument.Number, insuranceDocument.BeginDate.ToString("dd.MM.yyyy"),
                         insuranceDocument.EndDate.ToString("dd.MM.yyyy"));
                }
                return resStr;
            }
        }

        private ObservableCollection<InsuranceDocumentType> listInsuranceDocumentTypes = new ObservableCollection<InsuranceDocumentType>();
        public ObservableCollection<InsuranceDocumentType> ListInsuranceDocumentTypes
        {
            get { return listInsuranceDocumentTypes; }
            set { Set("ListInsuranceDocumentTypes", ref listInsuranceDocumentTypes, value); }
        }

        #endregion

        #region Commands

        public ICommand AddInsuranceDocumentCommand { get; set; }
        private void AddInsuranceDocument()
        {
            InsuranceDocuments.Add(new InsuranceDocumentViewModel(new InsuranceDocument() {BeginDate = new DateTime(DateTime.Now.Year, 1, 1) }, service));
            RaisePropertyChanged("PersonInsuranceDocumentsHasNoItems");
        }

        public ICommand DeleteInsuranceDocumentCommand { get; set; }
        private void DeleteInsuranceDocument(InsuranceDocumentViewModel insuranceDocument)
        {
            InsuranceDocuments.Remove(insuranceDocument);
            RaisePropertyChanged("PersonInsuranceDocumentsHasNoItems");
        }

        #endregion

        #region Implementation IDialogViewModel

        public string Title
        {
            get { return "Страховые документы"; }
        }

        public string ConfirmButtonText
        {
            get { return "Применить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        private bool saveWasRequested;

        public RelayCommand<object> CloseCommand { get; set; }

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                var notEroors = true;
                foreach (var insurancedocumentsViewModel in InsuranceDocuments)
                {
                    notEroors &= insurancedocumentsViewModel.Invalidate();
                }
                if (notEroors)
                {
                    OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        public List<InsuranceDocument> GetUnsavedPersonInsuranceDocuments()
        {
            List<InsuranceDocument> listInsuranceDocument = new List<InsuranceDocument>();
            InsuranceDocument personInsuranceDocument = null;
            foreach (var insurancedocumentsViewModel in InsuranceDocuments)
            {
                personInsuranceDocument = insurancedocumentsViewModel.SetData();
                //personInsuranceDocument.PersonId = personId;
                listInsuranceDocument.Add(personInsuranceDocument);
            }
            return listInsuranceDocument;
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
