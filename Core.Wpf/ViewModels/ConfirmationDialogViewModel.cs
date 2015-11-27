using System;
using System.Windows.Navigation;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;

namespace Core.Wpf.ViewModels
{
    public sealed class ConfirmationDialogViewModel : BindableBase, IDialogViewModel
    {
        public ConfirmationDialogViewModel()
        {
            title = "Подтверждение";
            confirmButtonText = "Да";
            cancelButtonText = "Нет";
            CloseCommand = new DelegateCommand<bool?>(x => OnCloseRequested(x == true));
        }

        private string question;

        public string Question
        {
            get { return question; }
            set { SetProperty(ref question, value); }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string confirmButtonText;

        public string ConfirmButtonText
        {
            get { return confirmButtonText; }
            set { SetProperty(ref confirmButtonText, value); }
        }

        private string cancelButtonText;

        public string CancelButtonText
        {
            get { return cancelButtonText; }
            set { SetProperty(ref cancelButtonText, value); }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        private void OnCloseRequested(bool dialogResult)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, new ReturnEventArgs<bool>(dialogResult));
            }
        }
    }
}