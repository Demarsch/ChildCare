using System;
using GalaSoft.MvvmLight;

namespace AdminTools.DTO
{
    public class PersonDTO : ObservableObject
    {
        public int Id { get; set; }

        private string fullName;
        public string FullName
        {
            get { return fullName; }
            set { Set("FullName", ref fullName, value); }
        }

        private DateTime birthDate;
        public DateTime BirthDate
        {
            get { return birthDate; }
            set { Set("BirthDate", ref birthDate, value); }
        }

        private string snils;
        public string Snils
        {
            get { return snils; }
            set { Set("Snils", ref snils, value); }
        }

        public string EditImage { get; set; }

        public string SynchImage { get; set; }

        private string sid;
        public string SID
        {
            get { return sid; }
            set { Set("SID", ref sid, value); }
        }

        private string principalName;
        public string PrincipalName
        {
            get { return principalName; }
            set { Set("PrincipalName", ref principalName, value); }
        }

        private bool? enabled;
        public bool? Enabled
        {
            get { return enabled; }
            set { Set("Enabled", ref enabled, value); }

        }
    }
}
