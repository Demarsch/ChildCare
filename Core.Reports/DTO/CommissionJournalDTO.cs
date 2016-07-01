using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reports.DTO
{
    public class CommissionJournalDTO
    {
        public int Id;
        public int CommissionNumber;
        public int ProtocolNumber;
        public string CommissionDate;
        public string AssignPerson;
        public string PatientFIO;
        public string PatientBirthDate;
        public string CardNumber;
        public string BranchName;
        public string PatientGender;
        public string PatientSocialStatus;
        public string PatientDiagnos;
        public string CommissionGroup;
        public int CommissionTypeId;
        public string CommissionType;
        public int CommissionQuestionId;
        public string CommissionName;
        public string Decision;
        public string Recommendations;
        public string Details;
        public string Experts;
    }
}
