using System;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class PersonViewModel : ObservableObject
    {
        public bool IsEmpty
        {
            get { return patient == null; }
        }

        private readonly Person patient;

        public PersonViewModel(Person patient)
        {
            this.patient = patient;
        }

        public int Id
        {
            get { return IsEmpty ? 0 : patient.Id; }
        }

        public string FullName
        {
            get { return IsEmpty ? string.Empty : patient.FullName; }
        }

        //TODO: rework into comparing with named constant
        public bool IsMale
        {
            get { return IsEmpty || patient.GenderId == 1; }
        }

        public string Gender
        {
            get { return (IsEmpty || patient.Gender == null) ? string.Empty : patient.Gender.ShortName; }
        }

        public DateTime BirthDate
        {
            get { return IsEmpty ? DateTime.MinValue : patient.BirthDate; }
        }

        public string Snils
        {
            get { return IsEmpty ? string.Empty : patient.Snils; }
        }

        public string MedNumber
        {
            get { return IsEmpty ? string.Empty : patient.MedNumber; }
        }

        public int AmbNumber
        {
            get { return IsEmpty ? 0 : patient.AmbNumber; }
        }

        public string AmbNumberString
        {
            get { return IsEmpty ? string.Empty : patient.AmbNumberString; }
        }

        public bool AmbNumberExist
        {
            get { return AmbNumber > 0; }
        }

        //TODO: rework into loading photo from base. Probably worth using IsAsync binding property
        public string PhotoSource
        {
            get
            {
                return IsEmpty
                    ? "pack://application:,,,/Resources;component/Images/Question48x48.png"
                    : IsMale
                        ? "pack://application:,,,/Resources;component/Images/Man48x48.png"
                        : "pack://application:,,,/Resources;component/Images/Woman48x48.png";
            }
        }

        public string ShortName { get { return patient.ShortName; } }
    }
}
