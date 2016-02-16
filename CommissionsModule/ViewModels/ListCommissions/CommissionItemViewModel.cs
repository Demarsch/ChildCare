using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommissionsModule.ViewModels
{
    public class CommissionItemViewModel: BindableBase
    {
        public CommissionItemViewModel()
        {
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set { SetProperty(ref personId, value); }
        }

        private int? decisionId;
        public int? DecisionId
        {
            get { return decisionId; }
            set { SetProperty(ref decisionId, value); }
        }

        private string decisionText;
        public string DecisionText
        {
            get { return decisionText; }
            set { SetProperty(ref decisionText, value); }
        }

        private string decisionColorHex;
        public string DecisionColorHex
        {
            get { return decisionColorHex; }
            set { SetProperty(ref decisionColorHex, value); }
        }

        private string protocolNumber;
        public string ProtocolNumber
        {
            get { return protocolNumber; }
            set { SetProperty(ref protocolNumber, value); }
        }  

        private bool? isCompleted;
        public bool? IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }

        private string patientFIO;
        public string PatientFIO
        {
            get { return patientFIO; }
            set { SetProperty(ref patientFIO, value); }
        }

        private string birthDate;
        public string BirthDate
        {
            get { return birthDate; }
            set { SetProperty(ref birthDate, value); }
        }

        private string talon;
        public string Talon
        {
            get { return talon; }
            set { SetProperty(ref talon, value); }
        }

        private string medHelpType;
        public string MedHelpType
        {
            get { return medHelpType; }
            set { SetProperty(ref medHelpType, value); }
        }

        private string mkb;
        public string MKB
        {
            get { return mkb; }
            set { SetProperty(ref mkb, value); }
        }

        private string incomeDateTime;
        public string IncomeDateTime
        {
            get { return incomeDateTime; }
            set { SetProperty(ref incomeDateTime, value); }
        }
    }
}
