﻿using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using MainLib;
using System.ComponentModel;
using System.Windows.Media;

namespace Registry
{
    public class EditPersonDataViewModel : ObservableObject, IDataErrorInfo
    {
        private readonly ILog log;

        private readonly IPersonService service;

        private readonly IDialogService dialogService;

        private readonly IDocumentService documentService;

        private Person person;

        /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonDataViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            if (documentService == null)
                throw new ArgumentNullException("documentService");
            this.documentService = documentService;
            this.dialogService = dialogService;
            this.service = service;
            this.log = log;
            commonPersonData = new EditPersonCommonDataViewModel(null, service, dialogService, documentService);
            CommonPersonData.PropertyChanged += CommonPersonData_PropertyChanged;
            EditInsuranceCommand = new RelayCommand(EditInsurance);
            EditPersonAddressCommand = new RelayCommand(EditPersonAddress);
            EditPersonIdentityDocumentsCommand = new RelayCommand(EditPersonIdentityDocuments);
            EditPersonDisabilitiesCommand = new RelayCommand(EditPersonDisabilities);
            EditPersonSocialStatusesCommand = new RelayCommand(EditPersonSocialStatuses);
            HealthGroups = new ObservableCollection<HealthGroup>(service.GetHealthGroups());
            MaritalStatuses = new ObservableCollection<MaritalStatus>(service.GetMaritalStatuses());
            Educations = new ObservableCollection<Education>(service.GetEducations());
            Countries = new ObservableCollection<Country>(service.GetCountries());

        }

        public EditPersonDataViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService, int personId)
            : this(log, service, dialogService, documentService)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonDataViewModel(ILog log, IPersonService service, IDialogService dialogService, IDocumentService documentService, string personData)
            : this(log, service, dialogService, documentService)
        {

        }

        private EditPersonCommonDataViewModel commonPersonData = null;
        public EditPersonCommonDataViewModel CommonPersonData
        {
            get { return commonPersonData; }
            set { Set(() => CommonPersonData, ref commonPersonData, value); }
        }

        #region Properties for relatives

        private int relativeToPersonId = -1;
        public int RelativeToPersonId
        {
            get { return relativeToPersonId; }
            set { Set(() => RelativeToPersonId, ref relativeToPersonId, value); }
        }

        private string shortName;
        public string ShortName
        {
            get { return shortName; }
            set { Set(() => ShortName, ref shortName, value); }
        }

        private int relativeRelationId;
        public int RelativeRelationId
        {
            get { return relativeRelationId; }
            set { Set(() => RelativeRelationId, ref relativeRelationId, value); }
        }

        private bool isRepresentative;
        public bool IsRepresentative
        {
            get { return isRepresentative; }
            set { Set(() => IsRepresentative, ref isRepresentative, value); }
        }

        private ImageSource photoUri;
        public ImageSource PhotoUri
        {
            get { return photoUri; }
            set { Set(() => PhotoUri, ref photoUri, value); }
        }

        #endregion

        private void FillPropertyFromPerson()
        {
            var dateTimeNow = DateTime.Now;

        }

        public async void GetPersonData()
        {
            var task = Task.Factory.StartNew(GetPersonDataAsync);
            await task;
            FillPropertyFromPerson();
        }

        private void GetPersonDataAsync()
        {
            var dateTimeNow = DateTime.Now;
            person = service.GetPersonById(Id);
            if (!IsEmpty)
            {
                CommonPersonData.Person = person;
                HealthGroupId = service.GetHealthGroupId(person.Id, dateTimeNow);
                NationalityId = service.GetNationalityCountryId(person.Id, dateTimeNow);
                MaritalStatusId = service.GetMaritalStatusId(person.Id, dateTimeNow);
                EducationId = service.GetEducationId(person.Id, dateTimeNow);
                if (NationalityId < 1)
                    NationalityId = service.GetDefaultNationalityCountryId();
                Insurances = service.GetActualInsuranceDocumentsString(Id);
                Addresses = service.GetActualPersonAddressesString(Id);
                IdentityDocuments = service.GetActualPersonIdentityDocumentsString(Id);
                Disabilities = service.GetActualPersonDisabilitiesString(Id);
                SocialStatuses = service.GetActualPersonSocialStatusesString(Id);
            }
            // if relative
            var personRelative = service.GetPersonRelative(RelativeToPersonId, Id);
            if (personRelative != null)
            {
                IsRepresentative = personRelative.IsRepresentative;
                RelativeRelationId = personRelative.RelativeRelationshipId;
                ShortName = person.ShortName;
                PhotoUri = CommonPersonData.PhotoURI;
            }
            else
            {
                IsRepresentative = false;
                RelativeRelationId = 0;
                ShortName = string.Empty;
                PhotoUri = null;
            }
        }

        void CommonPersonData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BirthDate")
            {
                RaisePropertyChanged(() => IsChild);
            }
            if (e.PropertyName == "FirstName" || e.PropertyName == "LastName" || e.PropertyName == "MiddleName")
            {
                ShortName = CommonPersonData.LastName + " " + (CommonPersonData.FirstName != string.Empty ? CommonPersonData.FirstName.Substring(0, 1) + ". " : string.Empty) +
                    (CommonPersonData.MiddleName != string.Empty ? CommonPersonData.MiddleName.Substring(0, 1) + "." : string.Empty);
            }
            if (e.PropertyName == "PhotoURI")
            {
                PhotoUri = CommonPersonData.PhotoURI;
            }
        }

        public ICommand EditInsuranceCommand { get; set; }
        private PersonInsuranceDocumentsViewModel insuranceDocumentViewModel;
        private void EditInsurance()
        {
            if (insuranceDocumentViewModel == null)
                insuranceDocumentViewModel = new PersonInsuranceDocumentsViewModel(Id, log, service, dialogService);
            var dialogResult = dialogService.ShowDialog(insuranceDocumentViewModel);
            if (dialogResult != true)
                insuranceDocumentViewModel = new PersonInsuranceDocumentsViewModel(Id, log, service, dialogService);
            Insurances = insuranceDocumentViewModel.ActialInsuranceDocumentsString;
        }

        public ICommand EditPersonAddressCommand { get; set; }
        private PersonAddressesViewModel personAddressesViewModel = null;
        private void EditPersonAddress()
        {
            if (personAddressesViewModel == null)
                personAddressesViewModel = new PersonAddressesViewModel(Id, service, dialogService);
            var dialogResult = dialogService.ShowDialog(personAddressesViewModel);
            if (dialogResult != true)
                personAddressesViewModel = new PersonAddressesViewModel(Id, service, dialogService);
            Addresses = personAddressesViewModel.PersonAddressesString;
        }

        public ICommand EditPersonIdentityDocumentsCommand { get; set; }
        private PersonIdentityDocumentsViewModel personIdentityDocumentsViewModel = null;
        private void EditPersonIdentityDocuments()
        {
            if (personIdentityDocumentsViewModel == null)
                personIdentityDocumentsViewModel = new PersonIdentityDocumentsViewModel(Id, service, dialogService);
            var dialogResult = dialogService.ShowDialog(personIdentityDocumentsViewModel);
            if (dialogResult != true)
                personIdentityDocumentsViewModel = new PersonIdentityDocumentsViewModel(Id, service, dialogService);
            IdentityDocuments = personIdentityDocumentsViewModel.PersonIdentityDocumentsString;
        }

        public ICommand EditPersonDisabilitiesCommand { get; set; }
        private PersonDisabilitiesViewModel personDisabilitiesViewModel = null;
        private void EditPersonDisabilities()
        {
            if (personDisabilitiesViewModel == null)
                personDisabilitiesViewModel = new PersonDisabilitiesViewModel(Id, service, dialogService);
            var dialogResult = dialogService.ShowDialog(personDisabilitiesViewModel);
            if (dialogResult != true)
                personDisabilitiesViewModel = new PersonDisabilitiesViewModel(Id, service, dialogService);
            Disabilities = personDisabilitiesViewModel.PersonDisabilitiesString;
        }

        public ICommand EditPersonSocialStatusesCommand { get; set; }
        private PersonSocialStatusesViewModel personSocialStatusesViewModel = null;
        private void EditPersonSocialStatuses()
        {
            if (personSocialStatusesViewModel == null)
                personSocialStatusesViewModel = new PersonSocialStatusesViewModel(Id, service, dialogService);
            var dialogResult = dialogService.ShowDialog(personSocialStatusesViewModel);
            if (dialogResult != true)
                personSocialStatusesViewModel = new PersonSocialStatusesViewModel(Id, service, dialogService);
            SocialStatuses = personSocialStatusesViewModel.PersonSocialStatusesString;
        }

        private ObservableCollection<HealthGroup> healthGroups = new ObservableCollection<HealthGroup>();
        public ObservableCollection<HealthGroup> HealthGroups
        {
            get { return healthGroups; }
            set { Set(() => HealthGroups, ref healthGroups, value); }
        }

        private ObservableCollection<Country> countries = new ObservableCollection<Country>();
        public ObservableCollection<Country> Countries
        {
            get { return countries; }
            set { Set(() => Countries, ref countries, value); }
        }

        private ObservableCollection<Education> educations = new ObservableCollection<Education>();
        public ObservableCollection<Education> Educations
        {
            get { return educations; }
            set { Set(() => Educations, ref educations, value); }
        }

        private ObservableCollection<MaritalStatus> maritalStatuses = new ObservableCollection<MaritalStatus>();
        public ObservableCollection<MaritalStatus> MaritalStatuses
        {
            get { return maritalStatuses; }
            set { Set(() => MaritalStatuses, ref maritalStatuses, value); }
        }

        private string textMessage = string.Empty;
        public string TextMessage
        {
            get { return textMessage; }
            set { Set(() => TextMessage, ref textMessage, value); }
        }

        public bool IsEmpty
        {
            get { return person == null; }
        }

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                Set(() => Id, ref id, value);
                GetPersonData();
            }
        }

        private int educationId = 0;
        public int EducationId
        {
            get { return educationId; }
            set { Set(() => EducationId, ref educationId, value); }
        }

        private int maritalStatusId = 0;
        public int MaritalStatusId
        {
            get { return maritalStatusId; }
            set { Set(() => MaritalStatusId, ref maritalStatusId, value); }
        }

        private int healthGroupId = 0;
        public int HealthGroupId
        {
            get { return healthGroupId; }
            set { Set(() => HealthGroupId, ref healthGroupId, value); }
        }

        private int nationalityId = 0;
        public int NationalityId
        {
            get { return nationalityId; }
            set { Set(() => NationalityId, ref nationalityId, value); }
        }

        private string phones = string.Empty;
        public string Phones
        {
            get { return phones; }
            set { Set(() => Phones, ref phones, value); }
        }

        private string insurances = string.Empty;
        public string Insurances
        {
            get { return insurances; }
            set { Set(() => Insurances, ref insurances, value); }
        }

        private string addresses = string.Empty;
        public string Addresses
        {
            get { return addresses; }
            set { Set(() => Addresses, ref addresses, value); }
        }

        private string identityDocuments = string.Empty;
        public string IdentityDocuments
        {
            get { return identityDocuments; }
            set { Set(() => IdentityDocuments, ref identityDocuments, value); }
        }

        private string disabilities = string.Empty;
        public string Disabilities
        {
            get { return disabilities; }
            set { Set(() => Disabilities, ref disabilities, value); }
        }

        private string relatives = string.Empty;
        public string Relatives
        {
            get { return relatives; }
            set { Set(() => Relatives, ref relatives, value); }
        }

        private string socialStatuses = string.Empty;
        public string SocialStatuses
        {
            get { return socialStatuses; }
            set { Set(() => SocialStatuses, ref socialStatuses, value); }
        }

        public string PersonFullName
        {
            get { return CommonPersonData.FullName; }
        }

        public bool IsChild
        {
            get
            {
                var isChild = CommonPersonData.BirthDate.AddYears(18) > DateTime.Now;
                if (!isChild)
                    HealthGroupId = 0;
                return isChild;
            }
        }

        #region Methods

        public List<InsuranceDocument> GetUnsavedPersonInsuranceDocuments()
        {
            if (insuranceDocumentViewModel != null)
                return insuranceDocumentViewModel.GetUnsavedPersonInsuranceDocuments();
            return null;
        }

        public List<PersonAddress> GetUnsavedPersonAddresses()
        {
            if (personAddressesViewModel != null)
                return personAddressesViewModel.GetUnsavedPersonAddresses();
            return null;
        }

        public Person GetUnsavedPerson(out List<PersonName> personNames)
        {
            return CommonPersonData.SetPerson(out personNames);
        }

        public List<PersonIdentityDocument> GetUnsavedPersonIdentityDocuments()
        {
            if (personIdentityDocumentsViewModel != null)
                return personIdentityDocumentsViewModel.GetUnsavedPersonIdentityDocuments();
            return null;
        }

        public List<PersonDisability> GetUnsavedPersonDisabilities()
        {
            if (personDisabilitiesViewModel != null)
                return personDisabilitiesViewModel.GetUnsavedPersonDisabilities();
            return null;
        }

        public List<PersonSocialStatus> GetUnsavedPersonSocialStatuses()
        {
            if (personSocialStatusesViewModel != null)
                return personSocialStatusesViewModel.GetUnsavedPersonSocialStatuses();
            return null;
        }

        #endregion

        #region Inplementation IDataErrorInfo

        private bool saveWasRequested { get; set; }

        public readonly HashSet<string> invalidProperties = new HashSet<string>();

        public bool Invalidate()
        {
            saveWasRequested = true;
            RaisePropertyChanged(string.Empty);
            return CommonPersonData.Invalidate() && invalidProperties.Count < 1;
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
                if (columnName == "HealthGroupId")
                {
                    result = IsChild && HealthGroupId < 1 ? "Укажите группу здоровья" : string.Empty;
                }
                if (columnName == "NationalityId")
                {
                    result = NationalityId < 1 ? "Укажите гражданство" : string.Empty;
                }
                if (columnName == "RelativeRelationId")
                {
                    result = RelativeRelationId < 1 ? "Укажите степень родства" : string.Empty;
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
