using System.ComponentModel;
using System.Threading.Tasks;
using Core.Data;
using Core.Wpf.Misc;

namespace PatientInfoModule.Misc
{
    public interface IAddressSuggestionProvider : INotifyPropertyChanged
    {
        ISuggestionProvider RegionSuggestionProvider { get; }

        Okato SelectedRegion { get; set; }

        ISuggestionProvider LocationSuggestionProvider { get; }

        Task EnsureDataSourceLoadedAsync();
    }
}
