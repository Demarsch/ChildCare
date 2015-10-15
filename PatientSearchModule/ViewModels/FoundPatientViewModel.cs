using System;
using Core.Data;
using Prism.Mvvm;

namespace PatientSearchModule.ViewModels
{
    public class FoundPatientViewModel : BindableBase
    {
        private DateTime birthDate;

        public DateTime BirthDate
        {
            get { return birthDate; }
            set { SetProperty(ref birthDate, value); }
        }

        private string snils;

        public string Snils
        {
            get { return snils; }
            set { SetProperty(ref snils, value); }
        }

        private string medNumber;

        public string MedNumber
        {
            get { return medNumber; }
            set { SetProperty(ref medNumber, value); }
        }

        private PersonName currentNames;

        public PersonName CurrentNames
        {
            get { return currentNames; }
            set { SetProperty(ref currentNames, value); }
        }

        private PersonName previousName;

        public PersonName PreviousName
        {
            get { return previousName; }
            set { SetProperty(ref previousName, value); }
        }

        private PersonIdentityDocument identityDocument;

        public PersonIdentityDocument IdentityDocument
        {
            get { return identityDocument; }
            set { SetProperty(ref identityDocument, value); }
        }
    }
}
