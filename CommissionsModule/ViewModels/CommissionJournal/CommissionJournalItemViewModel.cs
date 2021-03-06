﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommissionsModule.ViewModels
{
    public class CommissionJournalItemViewModel : BindableBase
    {
        public CommissionJournalItemViewModel()
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

        private int commissionNumber;
        public int CommissionNumber
        {
            get { return commissionNumber; }
            set { SetProperty(ref commissionNumber, value); }
        }

        private int protocolNumber;
        public int ProtocolNumber
        {
            get { return protocolNumber; }
            set { SetProperty(ref protocolNumber, value); }
        }

        private string commissionDate;
        public string CommissionDate
        {
            get { return commissionDate; }
            set { SetProperty(ref commissionDate, value); }
        }

        private string assignPerson;
        public string AssignPerson
        {
            get { return assignPerson; }
            set { SetProperty(ref assignPerson, value); }
        }

        private string patientFIO;
        public string PatientFIO
        {
            get { return patientFIO; }
            set { SetProperty(ref patientFIO, value); }
        }

        private string patientBirthDate;
        public string PatientBirthDate
        {
            get { return patientBirthDate; }
            set { SetProperty(ref patientBirthDate, value); }
        }

        private string cardNumber;
        public string CardNumber
        {
            get { return cardNumber; }
            set { SetProperty(ref cardNumber, value); }
        }

        private string branchName;
        public string BranchName
        {
            get { return branchName; }
            set { SetProperty(ref branchName, value); }
        }

        private string patientGender;
        public string PatientGender
        {
            get { return patientGender; }
            set { SetProperty(ref patientGender, value); }
        }

        private string patientSocialStatus;
        public string PatientSocialStatus
        {
            get { return patientSocialStatus; }
            set { SetProperty(ref patientSocialStatus, value); }
        }

        private string patientDiagnos;
        public string PatientDiagnos
        {
            get { return patientDiagnos; }
            set { SetProperty(ref patientDiagnos, value); }
        }

        private string commissionGroup;
        public string CommissionGroup
        {
            get { return commissionGroup; }
            set { SetProperty(ref commissionGroup, value); }
        }

        private int commissionTypeId;
        public int CommissionTypeId
        {
            get { return commissionTypeId; }
            set { SetProperty(ref commissionTypeId, value); }
        }

        private string commissionType;
        public string CommissionType
        {
            get { return commissionType; }
            set { SetProperty(ref commissionType, value); }
        }

        private int commissionQuestionId;
        public int CommissionQuestionId
        {
            get { return commissionQuestionId; }
            set { SetProperty(ref commissionQuestionId, value); }
        }

        private string commissionName;
        public string CommissionName
        {
            get { return commissionName; }
            set { SetProperty(ref commissionName, value); }
        }

        private string decision;
        public string Decision
        {
            get { return decision; }
            set { SetProperty(ref decision, value); }
        }

        private string recommendations;
        public string Recommendations
        {
            get { return recommendations; }
            set { SetProperty(ref recommendations, value); }
        }

        private string details;
        public string Details
        {
            get { return details; }
            set { SetProperty(ref details, value); }
        }

        private string experts;
        public string Experts
        {
            get { return experts; }
            set { SetProperty(ref experts, value); }
        }
    }
}
