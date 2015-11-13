using PatientInfoModule.Services;
using System.Collections;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class PersonSuggestionProvider : ISuggestionProvider
    {
        private readonly IPatientService service;

        public PersonSuggestionProvider(IPatientService service)
        {
            this.service = service;
        }

        public IEnumerable GetSuggestions(string filter)
        {            
            return service.GetPersonsByFullName(filter);
        }
    }
}
