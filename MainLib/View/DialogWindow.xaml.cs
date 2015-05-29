using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Core
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

        protected override void OnClosing(CancelEventArgs e)
        {
            var viewModel = DataContext as IDialogViewModel;
            if (viewModel != null && !viewModelRequestedClose)
            {
                e.Cancel = true;
                new DispatcherTimer(TimeSpan.FromMilliseconds(100.0), DispatcherPriority.ApplicationIdle, CallViewModelClose, Dispatcher);
            }
            base.OnClosing(e);
        }

        private void CallViewModelClose(object sender, EventArgs eventArgs)
        {
            var dispatcherTimer = (sender as DispatcherTimer);
            dispatcherTimer.Tick -= CallViewModelClose;
            dispatcherTimer.Stop();
            (DataContext as IDialogViewModel).CloseCommand.Execute(null);
        }
    }
}
