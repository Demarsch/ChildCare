﻿using System;
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
            var window = new DialogWindow { Owner = GetActiveWindow(), DataContext = dialogViewModel };
            return window.ShowDialog();
        }

        public void ShowError(string errorMessage)
        {
            MessageBox.Show(
                GetActiveWindow(),
                errorMessage,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(GetActiveWindow(), message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool? AskUser(string question, bool isWarning = false)
        {
            var result = MessageBox.Show(
                GetActiveWindow(),
                question,
                "Подтверждение",
                MessageBoxButton.YesNo,
                isWarning ? MessageBoxImage.Warning : MessageBoxImage.Question);
            return result == MessageBoxResult.Yes
                ? true
                : result == MessageBoxResult.No ? (bool?)false : null;
        }

        private Window GetActiveWindow()
        {
            return Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;
        }

        public string[] ShowOpenFileDialog(bool allowMultipleChoice)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Multiselect = allowMultipleChoice;
            dialog.Filter = "All files (*.*)|*.*|Office Files|*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx|Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|Text files (*.txt)|*.txt";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)            
                return dialog.FileNames;
            return new string[0];
        }
    }
}
