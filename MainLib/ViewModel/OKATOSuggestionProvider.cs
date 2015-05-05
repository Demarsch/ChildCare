using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls.Editors;

namespace Core
{
    public class OKATOSuggestionProvider : ISuggestionProvider
    {
        private IPersonService service;

        public OKATOSuggestionProvider(IPersonService service)
        {
            this.service = service;
            OkatoRegion = string.Empty;
        }

        public string OkatoRegion { get; set; }

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 2))
                return null;
            return service.GetOKATOByName(filter, OkatoRegion);
        }
    }
}
