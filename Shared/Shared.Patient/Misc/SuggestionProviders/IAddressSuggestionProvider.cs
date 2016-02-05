using System.ComponentModel;
using System.Threading.Tasks;
using Core.Data;
using Core.Wpf.Misc;

namespace Shared.Patient.Misc
{
    public interface IAddressSuggestionProvider : INotifyPropertyChanged
    {
        ISuggestionsProvider RegionSuggestionsProvider { get; }

        Okato SelectedRegion { get; set; }

        ISuggestionsProvider LocationSuggestionsProvider { get; }

        Task EnsureDataSourceLoadedAsync();
    }
}
