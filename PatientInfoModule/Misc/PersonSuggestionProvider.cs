using PatientInfoModule.Services;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class PersonSuggestionProvider : ISuggestionProvider
    {
        private IPatientService service;

        public PersonSuggestionProvider(IPatientService service)
        {
            this.service = service;
        }

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;            

            return service.GetPersonsByFullName(filter);            
        }
    }
}
