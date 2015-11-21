using System;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Core.Wpf.Mvvm;
using Core.Wpf.Views;

namespace Core.Wpf.Services
{
    public class WindowsDialogServiceAsync : IDialogServiceAsync
    {
        private readonly ChildDialogWindow dialogWindow;

        private TaskCompletionSource<bool?> taskSource; 

        public WindowsDialogServiceAsync(ChildDialogWindow dialogWindow)
        {
            if (dialogWindow == null)
            {
                throw new ArgumentNullException("dialogWindow");
            }
            this.dialogWindow = dialogWindow;
        }

        public async Task<bool?> ShowDialogAsync(IDialogViewModel dialogViewModel)
        {
            if (dialogViewModel == null)
            {
                throw new ArgumentNullException("dialogViewModel");
            }
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
    }
}