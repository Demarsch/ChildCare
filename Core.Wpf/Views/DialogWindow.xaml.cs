using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Core.Wpf.Mvvm;

namespace Core.Wpf.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow
    {
        public DialogWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = e.NewValue as IDialogViewModel;
            if (viewModel == null)
            {
                throw new ArgumentException("This type of window allows only IDialogViewModel implementations as a data context");
            }
            viewModel.CloseRequested += OnCloseRequested;
        }

        private void OnCloseRequested(object sender, ReturnEventArgs<bool> returnEventArgs)
        {
            viewModelRequestedClose = true;
            DialogResult = returnEventArgs.Result;
            Close();
        }

        private bool viewModelRequestedClose;

        private DispatcherTimer timer;

        protected override void OnClosing(CancelEventArgs e)
        {
            var viewModel = DataContext as IDialogViewModel;
            if (viewModel != null && !viewModelRequestedClose)
            {
                e.Cancel = true;
                timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100.0), DispatcherPriority.ApplicationIdle, CallViewModelClose, Dispatcher);
            }
            base.OnClosing(e);
        }

        private void CallViewModelClose(object sender, EventArgs eventArgs)
        {
            timer.Tick -= CallViewModelClose;
            timer.Stop();
            (DataContext as IDialogViewModel).CloseCommand.Execute(null);
        }

        protected override void OnClosed(EventArgs e)
        {
            var viewModel = DataContext as IDialogViewModel;
            if (viewModel != null)
            {
                viewModel.CloseRequested -= OnCloseRequested;
            }
            base.OnClosed(e);
        }
    }
}
