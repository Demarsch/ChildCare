using System;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class PersonViewModel : ObservableObject
    {
        private readonly Person patient;

        public MainWindowViewModel PatientList { get; private set; }

        public PersonViewModel(Person patient, MainWindowViewModel patientList)
        {
            if (patient == null)
                throw new ArgumentNullException("patient");
            if (patientList == null)
                throw new ArgumentNullException("patientList");
            PatientList = patientList;
            this.patient = patient;
        }

        public int Id { get { return patient.Id; } }

        public string FullName { get { return patient.FullName; } }
        //TODO: rework into comparing with named constant
        public bool IsMale { get { return patient.GenderId == 1; } }

        public DateTime BirthDate { get { return patient.BirthDate; } }

        public string Snils { get { return patient.Snils; } }

        public string MedNumber { get { return patient.MedNumber; } }
    }
}
