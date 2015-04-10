using DataLib;
using System;
using GalaSoft.MvvmLight;
using Core;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace Registry
{
    class PersonInsuranceDocumentsViewModel : ObservableObject
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
            InsuranceDocuments = new ObservableCollection<InsuranceDocument>(service.GetInsuranceDocuments(this.personId));
            AddInsuranceDocumentCommand = new RelayCommand(AddInsuranceDocument);
        }

        private ObservableCollection<InsuranceDocument> insuranceDocuments = new ObservableCollection<InsuranceDocument>();
        public ObservableCollection<InsuranceDocument> InsuranceDocuments
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
            InsuranceDocuments.Add(new InsuranceDocument());
        }
    }
}
