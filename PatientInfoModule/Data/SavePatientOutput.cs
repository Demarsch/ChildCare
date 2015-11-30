using System.Collections.Generic;
using Core.Data;

namespace PatientInfoModule.Data
{
    public class SavePatientOutput
    {
        public PersonName Name { get; set; }
        
        public Person Person { get; set; }

        public PersonEducation Education { get; set; }

        public PersonMaritalStatus MaritalStatus { get; set; }

        public PersonHealthGroup HealthGroup { get; set; }

        public PersonNationality Nationality { get; set; }

        public ICollection<PersonIdentityDocument> IdentityDocuments { get; set; }

        public ICollection<InsuranceDocument> InsuranceDocuments { get; set; }
        
        public ICollection<PersonAddress> Addresses { get; set; }
        
        public ICollection<PersonDisability> DisabilityDocuments { get; set; }
        
        public ICollection<PersonSocialStatus> SocialStatuses { get; set; }

        public PersonRelative Relative { get; set; }
    }
}
