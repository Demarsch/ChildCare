using System.Collections;

namespace Core.Wpf.Misc
{
    public interface ISuggestionProvider
    {
        IEnumerable GetSuggestions(string filter);
    }
}