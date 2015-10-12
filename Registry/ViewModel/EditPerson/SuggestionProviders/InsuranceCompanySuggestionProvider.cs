using Core;
using WpfControls.Editors;

namespace Registry
{
    public class InsuranceCompanySuggestionProvider : ISuggestionProvider
    {
        private IPersonService service;

        public InsuranceCompanySuggestionProvider(IPersonService service)
        {
            this.service = service;
        }
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }
            if (filter.Length < 3)
            {
                return null;
            }

            return service.GetInsuranceCompanies(filter);
        }
    }
}
