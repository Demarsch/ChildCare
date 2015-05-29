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
        //The real value should be of type bool? but this type has problems with boxing/unboxing
        RelayCommand<object> CloseCommand { get; }

        event EventHandler<ReturnEventArgs<bool>> CloseRequested;
    }
}
