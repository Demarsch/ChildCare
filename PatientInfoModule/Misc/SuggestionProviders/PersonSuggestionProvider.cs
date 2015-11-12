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
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;
            return service.GetPersonsByFullName(filter);
        }
    }
}
