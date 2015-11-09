using System.ComponentModel;

namespace Core.Misc
{
    public interface IChangeTracker : INotifyPropertyChanged
    {
        bool IsEnabled { get; set; }

        bool HasChanges { get; }

        void AcceptChanges();

        void RestoreChanges();
    }
}
