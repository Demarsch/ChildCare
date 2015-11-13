using System;
using System.Collections.Generic;
using Core.Data;

namespace PatientInfoModule.Data
{
    public class SavePatientInput
    {
        public Person CurrentPerson { get; set; }

        public PersonName CurrentName { get; set; }

        public PersonName NewName { get; set; }
        
        public bool IsIncorrectName { get; set; }
        
        public bool IsNewName { get; set; }
        
        public DateTime NewNameStartDate { get; set; }

        public PersonEducation CurrentEducation { get; set; }

        public PersonEducation NewEducation { get; set; }

        public PersonMaritalStatus CurrentMaritalStatus { get; set; }

        public PersonMaritalStatus NewMaritalStatus { get; set; }

        public PersonHealthGroup CurrentHealthGroup { get; set; }

        public PersonHealthGroup NewHealthGroup { get; set; }

        public PersonNationality CurrentNationality { get; set; }

        public PersonNationality NewNationality { get; set; }

        public ICollection<PersonIdentityDocument> CurrentIdentityDocuments { get; set; }

        public ICollection<PersonIdentityDocument> NewIdentityDocuments { get; set; }

        public ICollection<InsuranceDocument> CurrentInsuranceDocuments { get; set; }

        public ICollection<InsuranceDocument> NewInsuranceDocuments { get; set; }

        public ICollection<PersonAddress> CurrentAddresses { get; set; }

        public ICollection<PersonAddress> NewAddresses { get; set; }

        public ICollection<PersonDisability> CurrentDisabilities { get; set; }

        public ICollection<PersonDisability> NewDisabilities { get; set; }
    }
}
