using System.ComponentModel;

namespace Core.Misc
{
    public interface IChangeTrackable : INotifyPropertyChanged
    {
        event PropertyChangedEventHandler BeforeTrackedPropertyChanged;
    }
}
