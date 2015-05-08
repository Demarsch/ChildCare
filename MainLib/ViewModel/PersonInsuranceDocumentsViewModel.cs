using DataLib;
using System;
using GalaSoft.MvvmLight;
using Core;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Linq;

namespace MainLib
{
    public class PersonInsuranceDocumentsViewModel : ObservableObject
    {
        private int personId;

        private IPersonService service;

        public PersonInsuranceDocumentsViewModel(int personId, IPersonService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.personId = personId;
            ListInsuranceDocumentTypes = new ObservableCollection<InsuranceDocumentType>(service.GetInsuranceDocumentTypes());
            InsuranceDocuments = new ObservableCollection<InsuranceDocumentViewModel>(service.GetInsuranceDocuments(this.personId).Select(x => new InsuranceDocumentViewModel(x)));
            AddInsuranceDocumentCommand = new RelayCommand(AddInsuranceDocument);
            DeleteInsuranceDocumentCommand = new RelayCommand<InsuranceDocumentViewModel>(DeleteInsuranceDocument);
            CancelCommand = new RelayCommand(Cancel);
            AcceptCommand = new RelayCommand(Accept);
            InsuranceCompanySuggestionProvider = new InsuranceCompanySuggestionProvider(service);
        }

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
            set { Set("InsuranceDocuments", ref insuranceDocuments, value); }
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
                    resStr += String.Format("тип док-та: {0}\r\nстрах. орг.: {1}\r\nсерия {2} номер {3}\r\nпериод действия {4}-{5}",
                         insuranceDocumentTypeName, insuranceDocument.InsuranceCompany.NameSMOK, insuranceDocument.Series, insuranceDocument.Number, insuranceDocument.BeginDate.ToString("dd.MM.yyyy"),
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

        public ICommand AddInsuranceDocumentCommand { get; set; }
        private void AddInsuranceDocument()
        {
            InsuranceDocuments.Add(new InsuranceDocumentViewModel(new InsuranceDocument()));
        }

        public ICommand DeleteInsuranceDocumentCommand { get; set; }
        private void DeleteInsuranceDocument(InsuranceDocumentViewModel insuranceDocument)
        {
            InsuranceDocuments.Remove(insuranceDocument);
        }

        private bool? isChangesAccepted;
        public bool? IsChangesAccepted
        {
            get { return isChangesAccepted; }
            set { Set("IsChangesAccepted", ref isChangesAccepted, value); }
        }

        public ICommand CancelCommand { get; set; }
        private void Cancel()
        {
            IsChangesAccepted = false;
        }

        public ICommand AcceptCommand { get; set; }
        private void Accept()
        {
            IsChangesAccepted = true;
        }
    }
}
