using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls.Editors;

namespace Core
{
    public class DisabilitiesGivenOrgSuggestionProvider : ISuggestionProvider
    {
        private IPersonService service;

        public DisabilitiesGivenOrgSuggestionProvider(IPersonService service)
        {
            this.service = service;
        }
        
        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;
            return service.GetDisabilitiesGivenOrgByName(filter);
        }
    }
}
