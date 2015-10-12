using WpfControls.Editors;

namespace Core
{
    public class PersonSuggestionProvider : ISuggestionProvider
    {
        private IPersonService service;

        public PersonSuggestionProvider(IPersonService service)
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
