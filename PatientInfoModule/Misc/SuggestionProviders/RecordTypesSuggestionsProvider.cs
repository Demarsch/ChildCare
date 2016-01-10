using Core.Wpf.Misc;
using PatientInfoModule.Services;

namespace PatientInfoModule.Misc
{
    public class RecordTypesSuggestionsProvider : ISuggestionsProvider
    {
        private readonly IRecordService service;

        public RecordTypesSuggestionsProvider(IRecordService service)
        {
            this.service = service;
        }

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
            {
                return null;
            }
            return service.GetRecordTypesByName(filter);            
        }
    }
}
