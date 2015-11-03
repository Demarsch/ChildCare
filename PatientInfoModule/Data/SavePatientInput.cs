using System;
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
    }
}
