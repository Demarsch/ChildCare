using System;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class PersonViewModel : ObservableObject
    {
        private readonly Person person;

        public PersonViewModel(Person person)
        {
            this.person = person;
        }

        public int Id { get { return IsEmpty ? 0 : person.Id; } }

        public string FullName { get { return IsEmpty ? string.Empty : person.FullName; } }
        //TODO: rework into comparing with named constant
        public bool IsMale { get { return IsEmpty || person.GenderId == 1; } }

        public DateTime BirthDate { get { return IsEmpty ? DateTime.MinValue : person.BirthDate; } }

        public string Snils { get { return IsEmpty ? string.Empty : person.Snils; } }

        public string MedNumber { get { return IsEmpty ? string.Empty : person.MedNumber; } }

        public bool IsEmpty { get { return person == null; } }
    }
}
