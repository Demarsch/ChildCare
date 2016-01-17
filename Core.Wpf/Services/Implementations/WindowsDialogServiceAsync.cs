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
    public class WindowsDialogServiceAsync : IDialogServiceAsync
    {
        private readonly IEventAggregator eventAggregator;

        private readonly ChildDialogWindow dialogWindow;

        private Stack<TaskCompletionSource<bool?>> taskSources;

        public WindowsDialogServiceAsync(IEventAggregator eventAggregator, ChildDialogWindow dialogWindow)
        {
            if (dialogWindow == null)
            {
                throw new ArgumentNullException("dialogWindow");
            }
            this.eventAggregator = eventAggregator;
            this.dialogWindow = dialogWindow;
            //This is for propert processing of multiple shows when one dialogviewmodel shows another one
            taskSources = new Stack<TaskCompletionSource<bool?>>();
        }

        public async Task<bool?> ShowDialogAsync(IDialogViewModel targetDialogViewModel, IDialogViewModel sourceDialogViewModel = null)
        {
            if (targetDialogViewModel == null)
            {
                throw new ArgumentNullException("targetDialogViewModel");
            }
            eventAggregator.GetEvent<MainMenuCloseRequestedEvent>().Publish(null);
            taskSources.Push(new TaskCompletionSource<bool?>());
            dialogWindow.DataContext = targetDialogViewModel;
            targetDialogViewModel.CloseRequested += DialogViewModelOnCloseRequested;
            dialogWindow.Show();
            dialogWindow.Focus();
            var result = await taskSources.Peek().Task;
            targetDialogViewModel.CloseRequested -= DialogViewModelOnCloseRequested;
            if (sourceDialogViewModel != null)
            {
                dialogWindow.DataContext = sourceDialogViewModel;
                dialogWindow.Show();
            }
            return result;
        }

        private void DialogViewModelOnCloseRequested(object sender, ReturnEventArgs<bool> returnEventArgs)
        {
            var taskSource = taskSources.Pop();
            taskSource.SetResult(returnEventArgs.Result);
        }
    }
}