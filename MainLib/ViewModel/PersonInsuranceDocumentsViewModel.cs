using DataLib;
using System;
using GalaSoft.MvvmLight;
using Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
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
    }
}
