using WpfControls.Editors;

namespace Core
{
    public class RecordTypesSuggestionProvider : ISuggestionProvider
    {
        private IRecordService service;

        public RecordTypesSuggestionProvider(IRecordService service)
        {
            this.service = service;
        }

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter) || (filter.Length < 3))
                return null;

            return service.GetRecordTypesByName(filter);            
        }
    }
}
