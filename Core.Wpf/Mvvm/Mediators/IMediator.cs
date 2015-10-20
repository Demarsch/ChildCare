using System.ComponentModel;

namespace Core.Wpf.Mvvm
{
    public interface IMediator : INotifyPropertyChanged
    {
        bool IsActive { get; }
    }
}
