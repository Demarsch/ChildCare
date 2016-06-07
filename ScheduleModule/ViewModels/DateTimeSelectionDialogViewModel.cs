using Core.Wpf.Mvvm;
using Prism.Mvvm;
using System;
using Prism.Commands;
using System.Windows.Navigation;

namespace ScheduleModule.ViewModels
{
    class DateTimeSelectionDialogViewModel : BindableBase, IDialogViewModel
    {
        public DateTimeSelectionDialogViewModel()
        {
            CloseCommand = new DelegateCommand<bool?>(Close);
            SelectedDateTime = DateTime.Now;
        }
        public string Title
        {
            get { return "Дополнительные детали назначения"; }
        }

        public string ConfirmButtonText
        {
            get { return "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отменить"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            OnCloseRequested(new ReturnEventArgs<bool>(validate.GetValueOrDefault()));
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private DateTime selectedDateTime;

        public DateTime SelectedDateTime
        {
            get { return selectedDateTime; }
            set
            {
                if (selectedDateTime != value)
                {
                    selectedDateTime = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
