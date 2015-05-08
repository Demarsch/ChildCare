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
            DialogResult = returnEventArgs.Result;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            var viewModel = DataContext as IDialogViewModel;
            if (viewModel != null)
            {
                viewModel.CloseRequested -= OnCloseRequested;
            }
        }
    }
}
