using System.Collections;

namespace Core.Wpf.Misc
{
    public interface ISuggestionsProvider
    {
        IEnumerable GetSuggestions(string filter);
    }
}