﻿using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using MainLib;
using System.ComponentModel;

namespace Registry
{
    public class EditPersonDataViewModel : ObservableObject, IDataErrorInfo
    {
        private readonly ILog log;

        private readonly IPersonService service;

        private readonly IDialogService dialogService;

        private Person person;

        /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonDataViewModel(ILog log, IPersonService service, IDialogService dialogService)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            if (dialogService == null)
                throw new ArgumentNullException("dialogService");
            this.dialogService = dialogService;
            this.service = service;
            this.log = log;
            commonPersonData = new EditPersonCommonDataViewModel(null, service, dialogService);
            CommonPersonData.PropertyChanged += CommonPersonData_PropertyChanged;
            SaveChangesCommand = new RelayCommand(SaveChanges);
            EditInsuranceCommand = new RelayCommand(EditInsurance);
            EditPersonAddressCommand = new RelayCommand(EditPersonAddress);
            EditPersonIdentityDocumentsCommand = new RelayCommand(EditPersonIdentityDocuments);
            EditPersonDisabilitiesCommand = new RelayCommand(EditPersonDisabilities);
            EditPersonSocialStatusesCommand = new RelayCommand(EditPersonSocialStatuses);
            HealthGroups = new ObservableCollection<HealthGroup>(service.GetHealthGroups());
            Countries = new ObservableCollection<Country>(service.GetCountries());

        }

        public EditPersonDataViewModel(ILog log, IPersonService service, IDialogService dialogService, int personId)
            : this(log, service, dialogService)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonDataViewModel(ILog log, IPersonService service, IDialogService dialogService, string personData)
            : this(log, service, dialogService)
        {

        }

        private ObservableCollection<InsuranceDocumentViewModel> insuranceDocuments;
        public ObservableCollection<InsuranceDocumentViewModel> InsuranceDocuments
        {
            get { return insuranceDocuments; }
            private set
            {
                if (value.Count < 1)
                    value.Add(new InsuranceDocumentViewModel(new InsuranceDocument(), service));
                Set("InsuranceDocuments", ref insuranceDocuments, value);
            }
        }

        private EditPersonCommonDataViewModel commonPersonData = null;
        public EditPersonCommonDataViewModel CommonPersonData
        {
            get { return commonPersonData; }
            set { Set(() => CommonPersonData, ref commonPersonData, value); }
        }

        private void FillPropertyFromPerson()
        {
            var dateTimeNow = DateTime.Now;
            if (!IsEmpty)
            {
                CommonPersonData.Person = person;
                HealthGroupId = service.GetHealthGroupId(person.Id, dateTimeNow);
                NationalityId = service.GetNationalityCountryId(person.Id, dateTimeNow);
                if (NationalityId < 1)
                    NationalityId = service.GetDefaultNationalityCountryId();
                Insurances = service.GetActualInsuranceDocumentsString(Id);
                Addresses = service.GetActualPersonAddressesString(Id);
                IdentityDocuments = service.GetActualPersonIdentityDocumentsString(Id);
                Disabilities = service.GetActualPersonDisabilitiesString(Id);
                SocialStatuses = service.GetActualPersonSocialStatusesString(Id);
            }
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
            person = service.GetPersonById(id);
        }

        void CommonPersonData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BirthDate")
            {
                RaisePropertyChanged(() => IsChild);
            }
        }

        public ICommand SaveChangesCommand { get; private set; }

        private void SaveChanges()
        {
            //List<PersonName> personNames = new List<PersonName>();
            //PersonName newPersonName = null;
            //if (IsFIOChanged)
            //{
            //    if (SelectedChangeNameReason == null)
            //    {
            //        TextMessage = "Не указана причина изменения ФИО";
            //        return;
            //    }
            //    if (SelectedChangeNameReason.NeedCreateNewPersonName && ChangeNameDate == null)
            //    {
            //        TextMessage = "Не указана дата изменения ФИО";
            //        return;
            //    }

            //    if (!SelectedChangeNameReason.NeedCreateNewPersonName)
            //    {
            //        personName.LastName = LastName;
            //        personName.FirstName = FirstName;
            //        personName.MiddleName = MiddleName;
            //    }
            //    else
            //    {
            //        newPersonName = new PersonName()
            //        {
            //            LastName = LastName,
            //            FirstName = FirstName,
            //            MiddleName = MiddleName,
            //            BeginDateTime = ChangeNameDate,
            //            EndDateTime = new DateTime(9000, 1, 1)
            //        };
            //        personNames.Add(newPersonName);

            //        personName.EndDateTime = ChangeNameDate;
            //        personName.ChangeNameReasonId = SelectedChangeNameReason.Id;
            //    }
            //}
            //else
            //{
            //    if (personName == null)
            //    {
            //        personName = new PersonName()
            //        {
            //            LastName = LastName,
            //            FirstName = FirstName,
            //            MiddleName = MiddleName,
            //            BeginDateTime = new DateTime(1900, 1, 1),
            //            EndDateTime = new DateTime(9000, 1, 1)
            //        };

            //    }
            //}
            //personNames.Add(personName);
            //if (!GenderId.HasValue)
            //{
            //    TextMessage = "Не указана пол";
            //    return;
            //}
            //if (IsEmpty)
            //{
            //    person = new Person();
            //}
            //person.BirthDate = BirthDate;
            //person.Snils = SNILS;
            //person.MedNumber = MedNumber;
            //person.GenderId = GenderId.Value;

            //person.ShortName = LastName + " " + FirstName.Substring(0, 1) + ". " + (MiddleName != string.Empty ? MiddleName.Substring(0, 1) + "." : string.Empty);
            //person.FullName = LastName + " " + FirstName + " " + MiddleName;

            ////InsuranceDocuments
            //var insuranceDocuments = insuranceDocumentViewModel.InsuranceDocuments.Select(x => new InsuranceDocument()
            //    {
            //        InsuranceCompanyId = x.InsuranceCompanyId,
            //        InsuranceDocumentTypeId = x.InsuranceDocumentTypeId,
            //        Number = x.Number,
            //        Series = x.Series,
            //        BeginDate = x.BeginDate,
            //        EndDate = x.EndDate
            //    }).ToList();
            ///*
            //var res = service.SetPersonInfoes(person, personNames, insuranceDocuments);
            //if (newPersonName != null)
            //    personName = newPersonName;
            //if (res == string.Empty)
            //    TextMessage = "Данные сохранены";
            //else
            //    TextMessage = "Ошибка! " + res;*/
            //RaisePropertyChanged("IsFIOChanged");
            //RaisePropertyChanged("IsSelectedChangeNameReasonWithCreateNewPersonNames");
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

        #region Inplementation IDataErrorInfo

        private bool saveWasRequested { get; set; }

        public readonly HashSet<string> invalidProperties = new HashSet<string>();

        public bool Invalidate()
        {
            saveWasRequested = true;
            RaisePropertyChanged(string.Empty);
            return invalidProperties.Count < 1;
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
