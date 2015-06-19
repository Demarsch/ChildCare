using System;
using GalaSoft.MvvmLight;

namespace Commission
{
    public class CommissionProtocolDTO : ObservableObject
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        private string patientFIO;
        public string PatientFIO
        {
            get { return patientFIO; }
            set { Set("PatientFIO", ref patientFIO, value); }
        }

        private string birthDate;
        public string BirthDate
        {
            get { return birthDate; }
            set { Set("BirthDate", ref birthDate, value); }
        }

        private string talon;
        public string Talon
        {
            get { return talon; }
            set { Set("Talon", ref talon, value); }
        }

        private string mkb;
        public string MKB
        {
            get { return mkb; }
            set { Set("MKB", ref mkb, value); }
        }

        private string incomeDateTime;
        public string IncomeDateTime
        {
            get { return incomeDateTime; }
            set { Set("IncomeDateTime", ref incomeDateTime, value); }
        }
    }
}
