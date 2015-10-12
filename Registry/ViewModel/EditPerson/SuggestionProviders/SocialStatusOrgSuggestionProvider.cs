using Core;
using WpfControls.Editors;

namespace Registry
{
    public class SocialStatusOrgSuggestionProvider : ISuggestionProvider
    {
        private IPersonService service;

        public SocialStatusOrgSuggestionProvider(IPersonService service)
        {
            this.service = service;
        }
        
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;
            return service.GetSocialStatusOrgByName(filter);
        }
    }
}
