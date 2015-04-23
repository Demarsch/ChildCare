using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls.Editors;

namespace Core
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
