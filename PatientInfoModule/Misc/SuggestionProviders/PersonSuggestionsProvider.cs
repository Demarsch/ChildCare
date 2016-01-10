using Core.Wpf.Misc;
using PatientInfoModule.Services;
using System.Collections;

namespace PatientInfoModule.Misc
{
    public class PersonSuggestionsProvider : ISuggestionsProvider
    {
        private readonly IPatientService service;

        public PersonSuggestionsProvider(IPatientService service)
        {
            this.service = service;
        }

        public IEnumerable GetSuggestions(string filter)
        {            
            return service.GetPersonsByFullName(filter);
        }
    }
}
