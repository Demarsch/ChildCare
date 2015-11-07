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
    }
}
