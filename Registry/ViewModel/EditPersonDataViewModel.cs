using DataLib;
using GalaSoft.MvvmLight;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry
{
    class EditPersonDataViewModel : ObservableObject
    {

        private readonly ILog log;
        private readonly MainService service;

        private Person person;

        /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonDataViewModel(ILog log, MainService service)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.log = log;
        }

        public EditPersonDataViewModel(ILog log, MainService service, int personId)
            : this(log, service)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            this.Id = personId;
            this.log = log;
        }

        /// <summary>
        /// Use this for creating new person with default data from search
        /// </summary>
        public EditPersonDataViewModel(ILog log, MainService service, string personData)
            : this(log, service)
        {

        }

        public bool IsEmpty
        {
            get { return person == null; }
        }

        public int Id
        {
            get { return person.Id; }
            set
            {
                if (person != null && person.Id == value)
                    return;
                person = service.GetPersonById(value);
            }
        }

        public string LastName
        {
            get
            {
                return IsEmpty ? string.Empty : person.CurrentLastName;
            }
        }

        public string FirstName
        {
            get
            {
                return IsEmpty ? string.Empty : person.CurrentFirstName;
            }
        }

        public string MiddleName
        {
            get
            {
                return IsEmpty ? string.Empty : person.CurrentMiddleName;
            }
        }

        public DateTime BirthDate
        {
            get
            {
                return IsEmpty ? DateTime.Parse("1/1/1900") : person.BirthDate;
            }
            set
            {
                if (person.BirthDate == value)
                    return;
                person.BirthDate = value;
                //Set("BirthDate", ref person.BirthDate, value);
            }
        }

        public string SNILS
        {
            get
            {
                return IsEmpty ? string.Empty : person.Snils;
            }
        }

        public string MedNumber
        {
            get
            {
                return IsEmpty ? string.Empty : person.MedNumber;
            }
        }

        public string Gender
        {
            get
            {
                return IsEmpty ? string.Empty : person.Gender.ShortName;
            }
        }
    }
}
