using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;

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
            if (viewModel != null)
            {
                e.Cancel = !viewModelRequestedClose && !viewModel.CanBeClosed();
                if (!e.Cancel)
                {
                    if (DialogResult == null)
                    {
                        DialogResult = true;
                    }
                    viewModel.CloseRequested -= OnCloseRequested;
                }
            }
            base.OnClosing(e);
        }
    }
}
