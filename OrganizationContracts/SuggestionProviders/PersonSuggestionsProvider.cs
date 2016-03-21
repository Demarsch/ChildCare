using Core.Wpf.Misc;
using OrganizationContractsModule.Services;
using System.Collections;

namespace OrganizationContractsModule.SuggestionsProviders
{
    public class PersonSuggestionsProvider : ISuggestionsProvider
    {
        private readonly IContractService service;

        public PersonSuggestionsProvider(IContractService service)
        {
            this.service = service;
        }

        public IEnumerable GetSuggestions(string filter)
        {            
            return service.GetPersonsByFullName(filter);
        }
    }
}
