using System;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class PersonViewModel : ObservableObject
    {
        private readonly Person patient;

        public PersonViewModel(Person patient)
        {
            if (patient == null)
                throw new ArgumentNullException("patient");
            this.patient = patient;
        }

        public int Id { get { return patient.Id; } }

        public string FullName { get { return patient.FullName; } }
        //TODO: rework into comparing with named constant
        public bool IsMale { get { return patient.GenderId == 1; } }

        public DateTime BirthDate { get { return patient.BirthDate; } }

        public string Snils { get { return patient.Snils; } }

        public string MedNumber { get { return patient.MedNumber; } }
        //TODO: rework into loading photo from base. Probably worth using IsAsync binding property
        public string PhotoSource { get
        {
            return IsMale
                ? "pack://application:,,,/Resources;component/Images/Man48x48.png"
                : "pack://application:,,,/Resources;component/Images/Woman48x48.png";
        } }
    }
}
