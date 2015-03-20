﻿using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Registry
{
    class EditPersonDataViewModel : ObservableObject
    {

        private readonly ILog log;
        private readonly IPatientService service;

        private Person person;

        /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonDataViewModel(ILog log, IPatientService service)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.log = log;
            SaveChangesCommand = new RelayCommand(SaveChanges);
        }

        public EditPersonDataViewModel(ILog log, IPatientService service, int personId)
            : this(log, service)
        {
            this.Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonDataViewModel(ILog log, IPatientService service, string personData)
            : this(log, service)
        {

        }

        private void FillPropertyFromPerson()
        {
            if (!IsEmpty)
            {
                LastName = person.CurrentLastName;
                FirstName = person.CurrentFirstName;
                MiddleName = person.CurrentMiddleName;
                BirthDate = person.BirthDate;
                SNILS = person.Snils;
                MedNumber = person.MedNumber;
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
            person = service.GetPersonById(id);
        }

        public ICommand SaveChangesCommand { get; private set; }

        private void SaveChanges()
        {
            throw new NotImplementedException();
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
                if (id == value)
                    return;
                id = value;
                GetPersonData();
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
            }
        }

        private DateTime birthDate;
        public DateTime BirthDate
        {
            get
            {
                return IsEmpty ? DateTime.Parse("1/1/1900") : birthDate;
            }
            set
            {
                Set("BirthDate", ref birthDate, value);
            }
        }

        private string snils = string.Empty;
        public string SNILS
        {
            get
            {
                return snils;
            }
            set
            {
                Set("SNILS", ref snils, value);
            }
        }

        private string medNumber = string.Empty;
        public string MedNumber
        {
            get
            {
                return medNumber;
            }
            set
            {
                Set("MedNumber", ref medNumber, value);
            }
        }

        // Maybe I need object?
        private string gender = string.Empty;
        public string Gender
        {
            get
            {
                return gender;
            }
            set
            {
                Set("Gender", ref gender, value);
            }
        }

        private string phones = string.Empty;
        public string Phones
        {
            get
            {
                return phones;
            }
            set
            {
                Set("Phones", ref phones, value);
            }
        }

        private string insurance = string.Empty;
        public string Insurance
        {
            get
            {
                return insurance;
            }
            set
            {
                Set("Insurance", ref insurance, value);
            }
        }

        private string addresses = string.Empty;
        public string Addresses
        {
            get
            {
                return addresses;
            }
            set
            {
                Set("Addresses", ref addresses, value);
            }
        }

        private string identityDocument = string.Empty;
        public string IdentityDocument
        {
            get
            {
                return identityDocument;
            }
            set
            {
                Set("IdentityDocument", ref identityDocument, value);
            }
        }

        private string relatives = string.Empty;
        public string Relatives
        {
            get
            {
                return relatives;
            }
            set
            {
                Set("Relatives", ref relatives, value);
            }
        }
    }
}
