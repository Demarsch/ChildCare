using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Core.Wpf.Events;
using Core.Wpf.Mvvm;
using Core.Wpf.Views;
using Prism.Events;

namespace Core.Wpf.Services
{
    public class WindowsDialogServiceAsync : IDialogServiceAsync, IDisposable
    {
        private readonly IEventAggregator eventAggregator;

        private readonly ChildDialogWindow dialogWindow;

        private TaskCompletionSource<bool?> taskSource;

        public WindowsDialogServiceAsync(IEventAggregator eventAggregator, ChildDialogWindow dialogWindow)
        {
            if (dialogWindow == null)
            {
                throw new ArgumentNullException("dialogWindow");
            }
            this.eventAggregator = eventAggregator;
            this.dialogWindow = dialogWindow;
        }

        public async Task<bool?> ShowDialogAsync(IDialogViewModel dialogViewModel)
        {
            if (dialogViewModel == null)
            {
                throw new ArgumentNullException("dialogViewModel");
            }
            eventAggregator.GetEvent<MainMenuCloseRequestedEvent>().Publish(null);
            taskSource = new TaskCompletionSource<bool?>();
            dialogWindow.DataContext = dialogViewModel;
            dialogViewModel.CloseRequested += DialogViewModelOnCloseRequested;
            dialogWindow.Show();
            var result = await taskSource.Task;
            dialogViewModel.CloseRequested -= DialogViewModelOnCloseRequested;
            return result;
        }

        private void DialogViewModelOnCloseRequested(object sender, ReturnEventArgs<bool> returnEventArgs)
        {
            taskSource.SetResult(returnEventArgs.Result);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}