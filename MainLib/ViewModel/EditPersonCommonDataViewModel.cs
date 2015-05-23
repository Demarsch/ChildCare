using Core;
using DataLib;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib
{
    public class EditPersonCommonDataViewModel : ObservableObject
    {
        #region Fields

        private readonly IPersonService service;


        private PersonName personName;

        #endregion

        #region Constructors

        public EditPersonCommonDataViewModel(Person person, IPersonService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            this.service = service;
            this.person = person;

            ChangeNameReasons = new ObservableCollection<ChangeNameReason>(service.GetActualChangeNameReasons());
            Genders = new ObservableCollection<Gender>(service.GetGenders());
        }

        #endregion

        #region Properties

        private Person person;
        public Person Person
        {
            get { return person; }
            set
            {
                Set(() => Person, ref person, value);
                if (!IsEmpty)
                {
                    FillPersonData();
                }
            }
        }

        public bool IsEmpty
        {
            get { return person == null; }
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

        private DateTime birthDate = DateTime.Parse("1/1/1900");
        public DateTime BirthDate
        {
            get { return birthDate; }
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

        public bool IsFIOChanged
        {
            get { return !IsEmpty && personName != null && (personName.LastName != LastName || personName.FirstName != FirstName || personName.MiddleName != MiddleName); }
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

        public string FullName
        {
            //ToDo: Maybe get current values of LastName, FirstName, MiddleName
            get { return !IsEmpty ? person.FullName : string.Empty; }
        }

        #endregion

        #region Methods

        private void FillPersonData()
        {
            if (!IsEmpty)
            {
                personName = service.GetActualPersonName(person.Id);
                if (personName != null)
                {
                    LastName = personName.LastName;
                    FirstName = personName.FirstName;
                    MiddleName = personName.MiddleName;
                }
                PhotoURI = person.PhotoUri;
                BirthDate = person.BirthDate;
                SNILS = person.Snils;
                MedNumber = person.MedNumber;
                GenderId = person.GenderId;
            }
            else
            {
                LastName = string.Empty;
                FirstName = string.Empty;
                MiddleName = string.Empty;
                BirthDate = new DateTime(1900, 1, 1);
                SNILS = string.Empty;
                MedNumber = string.Empty;
                GenderId = 0;
                PhotoURI = string.Empty;
            }
        }

        #endregion
    }
}
