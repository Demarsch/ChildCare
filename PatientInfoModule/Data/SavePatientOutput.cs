using Core.Data;

namespace PatientInfoModule.Data
{
    public class SavePatientOutput
    {
        public PersonName CurrentName { get; set; }
        
        public Person CurrentPerson { get; set; }
    }
}
