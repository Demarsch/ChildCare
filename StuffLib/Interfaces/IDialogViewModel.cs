using System;
using System.ComponentModel;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.CommandWpf;

namespace Core
{
    public interface IDialogViewModel : INotifyPropertyChanged
    {
        string Title { get; }

        string ConfirmButtonText { get; }

        string CancelButtonText { get; }

        RelayCommand<bool> CloseCommand { get; }

        bool CanBeClosed();

        event EventHandler<ReturnEventArgs<bool>> CloseRequested;
    }
}
