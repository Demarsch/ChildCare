using Core;
using WpfControls.Editors;

namespace Registry
{
    public class IdentityDocumentsGivenOrgSuggestionProvider : ISuggestionProvider
    {
        private IPersonService service;

        public IdentityDocumentsGivenOrgSuggestionProvider(IPersonService service)
        {
            this.service = service;
        }
        
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;
            return service.GetIdentityDocumentsGivenOrgByName(filter);
        }
    }
}
