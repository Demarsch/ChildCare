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
                Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow,
                errorMessage,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public bool? AskUser(string question, bool isWarning = false)
        {
            var result = MessageBox.Show(
                Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow,
                question,
                "Подтверждение",
                MessageBoxButton.YesNo,
                isWarning ? MessageBoxImage.Warning : MessageBoxImage.Question);
            return result == MessageBoxResult.Yes
                ? true
                : result == MessageBoxResult.No ? (bool?)false : null;
        }
    }
}
