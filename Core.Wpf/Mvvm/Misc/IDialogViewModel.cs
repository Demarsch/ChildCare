using System;
using System.ComponentModel;
using System.Windows.Navigation;
using Prism.Commands;

namespace Core.Wpf.Mvvm
{
    public interface IDialogViewModel : INotifyPropertyChanged
    {
        string Title { get; }

        string ConfirmButtonText { get; }

        string CancelButtonText { get; }
        
        DelegateCommand<bool?> CloseCommand { get; }

        event EventHandler<ReturnEventArgs<bool>> CloseRequested;
    }
}
