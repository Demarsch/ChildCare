using System;
using System.Linq;
using System.Windows;

namespace Core
{
    public class WindowDialogService : IDialogService
    {
        public bool? ShowDialog(IDialogViewModel dialogViewModel)
        {
            if (dialogViewModel == null)
            {
                throw new ArgumentNullException("dialogViewModel");
            }
            var window = new DialogWindow { Owner = Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive), DataContext = dialogViewModel };
            return window.ShowDialog();
        }

        public void ShowError(string errorMessage)
        {
            MessageBox.Show(
                Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive) ??
                Application.Current.MainWindow,
                errorMessage,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
