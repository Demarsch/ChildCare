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

namespace MainLib
{
    public class EditPersonDataViewModel : ObservableObject
    {
        private readonly ILog log;

        private readonly IPersonService service;

        private readonly IDialogService dialogService;

        private Person person;
        private PersonName personName;

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
            SaveChangesCommand = new RelayCommand(SaveChanges);
            EditInsuranceCommand = new RelayCommand(EditInsurance);
            EditPersonAddressCommand = new RelayCommand(EditPersonAddress);
            ChangeNameReasons = new ObservableCollection<ChangeNameReason>(service.GetActualChangeNameReasons());
            EditPersonIdentityDocumentsCommand = new RelayCommand(EditPersonIdentityDocuments);
            EditPersonDisabilitiesCommand = new RelayCommand(EditPersonDisabilities);
            Genders = new ObservableCollection<Gender>(service.GetGenders());

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

        private void FillPropertyFromPerson()
        {
            if (!IsEmpty)
            {
                if (personName != null)
                {
                    LastName = personName.LastName;
                    FirstName = personName.FirstName;
                    MiddleName = personName.MiddleName;
                }
                BirthDate = person.BirthDate;
                SNILS = person.Snils;
                MedNumber = person.MedNumber;
                GenderId = person.GenderId;
                PhotoURI = person.PhotoUri;
                Insurances = service.GetActualInsuranceDocumentsString(Id);
                Addresses = service.GetActualPersonAddressesString(Id);
                IdentityDocuments = service.GetActualPersonIdentityDocumentsString(Id);
                Disabilities = service.GetActualPersonDisabilitiesString(Id);
            }
            else
            {
                LastName = string.Empty;
                FirstName = string.Empty;
                MiddleName = string.Empty;
                BirthDate = new DateTime(1900, 1, 1);
                SNILS = string.Empty;
                MedNumber = string.Empty;
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
            if (!IsEmpty)
                personName = service.GetActualPersonName(id);
        }

        public ICommand SaveChangesCommand { get; private set; }

        private void SaveChanges()
        {
            List<PersonName> personNames = new List<PersonName>();
            PersonName newPersonName = null;
            if (IsFIOChanged)
            {
                if (SelectedChangeNameReason == null)
                {
                    TextMessage = "Не указана причина изменения ФИО";
                    return;
                }
                if (SelectedChangeNameReason.NeedCreateNewPersonName && ChangeNameDate == null)
                {
                    TextMessage = "Не указана дата изменения ФИО";
                    return;
                }

                if (!SelectedChangeNameReason.NeedCreateNewPersonName)
                {
                    personName.LastName = LastName;
                    personName.FirstName = FirstName;
                    personName.MiddleName = MiddleName;
                }
                else
                {
                    newPersonName = new PersonName()
                    {
                        LastName = LastName,
                        FirstName = FirstName,
                        MiddleName = MiddleName,
                        BeginDateTime = ChangeNameDate,
                        EndDateTime = new DateTime(9000, 1, 1)
                    };
                    personNames.Add(newPersonName);

                    personName.EndDateTime = ChangeNameDate;
                    personName.ChangeNameReasonId = SelectedChangeNameReason.Id;
                }
            }
            else
            {
                if (personName == null)
                {
                    personName = new PersonName()
                    {
                        LastName = LastName,
                        FirstName = FirstName,
                        MiddleName = MiddleName,
                        BeginDateTime = new DateTime(1900, 1, 1),
                        EndDateTime = new DateTime(9000, 1, 1)
                    };

                }
            }
            personNames.Add(personName);
            if (!GenderId.HasValue)
            {
                TextMessage = "Не указана пол";
                return;
            }
            if (IsEmpty)
            {
                person = new Person();
            }
            person.BirthDate = BirthDate;
            person.Snils = SNILS;
            person.MedNumber = MedNumber;
            person.GenderId = GenderId.Value;

            person.ShortName = LastName + " " + FirstName.Substring(0, 1) + ". " + (MiddleName != string.Empty ? MiddleName.Substring(0, 1) + "." : string.Empty);
            person.FullName = LastName + " " + FirstName + " " + MiddleName;

            //InsuranceDocuments
            var insuranceDocuments = insuranceDocumentViewModel.InsuranceDocuments.Select(x => new InsuranceDocument()
                {
                    InsuranceCompanyId = x.InsuranceCompanyId,
                    InsuranceDocumentTypeId = x.InsuranceDocumentTypeId,
                    Number = x.Number,
                    Series = x.Series,
                    BeginDate = x.BeginDate,
                    EndDate = x.EndDate
                }).ToList();
            /*
            var res = service.SetPersonInfoes(person, personNames, insuranceDocuments);
            if (newPersonName != null)
                personName = newPersonName;
            if (res == string.Empty)
                TextMessage = "Данные сохранены";
            else
                TextMessage = "Ошибка! " + res;*/
            RaisePropertyChanged("IsFIOChanged");
            RaisePropertyChanged("IsSelectedChangeNameReasonWithCreateNewPersonNames");
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

        private ObservableCollection<Gender> genders = new ObservableCollection<Gender>();
        public ObservableCollection<Gender> Genders
        {
            get { return genders; }
            set { Set("Genders", ref genders, value); }
        }

        private ObservableCollection<ChangeNameReason> сhangeNameReasons = new ObservableCollection<ChangeNameReason>();
        public ObservableCollection<ChangeNameReason> ChangeNameReasons
        {
            get { return сhangeNameReasons; }
            set { Set("ChangeNameReasons", ref сhangeNameReasons, value); }
        }

        private ChangeNameReason selectedChangeNameReason;
        public ChangeNameReason SelectedChangeNameReason
        {
            get { return selectedChangeNameReason; }
            set
            {
                Set("SelectedChangeNameReason", ref selectedChangeNameReason, value);
                RaisePropertyChanged("IsSelectedChangeNameReasonWithCreateNewPersonNames");
            }
        }

        public bool IsSelectedChangeNameReasonWithCreateNewPersonNames
        {
            get { return IsFIOChanged && SelectedChangeNameReason != null && SelectedChangeNameReason.NeedCreateNewPersonName; }
        }

        public DateTime changeNameDate;
        public DateTime ChangeNameDate
        {
            get { return changeNameDate; }
            set { Set("ChangeNameDate", ref changeNameDate, value); }
        }

        public bool IsFIOChanged
        {
            get { return !IsEmpty && personName != null && (personName.LastName != LastName || personName.FirstName != FirstName || personName.MiddleName != MiddleName); }
        }

        private string textMessage = string.Empty;
        public string TextMessage
        {
            get { return textMessage; }
            set { Set("TextMessage", ref textMessage, value); }
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
                Set("Id", ref id, value);
                GetPersonData();
            }
        }

        private string photoURI = string.Empty;
        public string PhotoURI
        {
            get
            {
                return photoURI;
            }
            set
            {
                string val = value;
                if (val == string.Empty)
                {
                    //ToDo: make this property connecteted with actual value
                    switch (GenderId)
                    {
                        case 1:
                            val = "pack://application:,,,/Resources;component/Images/Man48x48.png";
                            break;
                        case 2:
                            val = "pack://application:,,,/Resources;component/Images/Woman48x48.png";
                            break;
                        default:
                            val = "pack://application:,,,/Resources;component/Images/Question48x48.png";
                            break;
                    }
                }
                Set("PhotoURI", ref photoURI, val);

            }
        }

        private string lastName = string.Empty;
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                Set("LastName", ref lastName, value);
                RaisePropertyChanged("IsFIOChanged");
                RaisePropertyChanged("IsSelectedChangeNameReasonWithCreateNewPersonNames");
            }
        }

        private string firstName = string.Empty;
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                Set("FirstName", ref firstName, value);
                RaisePropertyChanged("IsFIOChanged");
                RaisePropertyChanged("IsSelectedChangeNameReasonWithCreateNewPersonNames");
            }
        }

        private string middleName = string.Empty;
        public string MiddleName
        {
            get
            {
                return middleName;
            }
            set
            {
                Set("MiddleName", ref middleName, value);
                RaisePropertyChanged("IsFIOChanged");
                RaisePropertyChanged("IsSelectedChangeNameReasonWithCreateNewPersonNames");
            }
        }

        private DateTime birthDate;
        public DateTime BirthDate
        {
            get { return IsEmpty ? DateTime.Parse("1/1/1900") : birthDate; }
            set { Set("BirthDate", ref birthDate, value); }
        }

        private string snils = string.Empty;
        public string SNILS
        {
            get { return snils; }
            set { Set("SNILS", ref snils, value); }
        }

        private string medNumber = string.Empty;
        public string MedNumber
        {
            get { return medNumber; }
            set { Set("MedNumber", ref medNumber, value); }
        }

        private int? genderId = 0;
        public int? GenderId
        {
            get { return genderId; }
            set { Set("GenderId", ref genderId, value); }
        }

        private string phones = string.Empty;
        public string Phones
        {
            get { return phones; }
            set { Set("Phones", ref phones, value); }
        }

        private string insurances = string.Empty;
        public string Insurances
        {
            get { return insurances; }
            set { Set("Insurances", ref insurances, value); }
        }

        private string addresses = string.Empty;
        public string Addresses
        {
            get { return addresses; }
            set { Set("Addresses", ref addresses, value); }
        }

        private string identityDocuments = string.Empty;
        public string IdentityDocuments
        {
            get { return identityDocuments; }
            set { Set("IdentityDocuments", ref identityDocuments, value); }
        }

        private string disabilities = string.Empty;
        public string Disabilities
        {
            get { return disabilities; }
            set { Set("Disabilities", ref disabilities, value); }
        }

        private string relatives = string.Empty;
        public string Relatives
        {
            get { return relatives; }
            set { Set("Relatives", ref relatives, value); }
        }
    }
}
