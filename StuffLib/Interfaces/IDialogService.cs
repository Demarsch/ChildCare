using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Forms;

namespace Core
{
    public interface IDialogService
    {
        bool? ShowDialog(IDialogViewModel dialogViewModel);

        void ShowError(string errorMessage);

        void ShowMessage(string message);

        bool? AskUser(string question, bool isWarning = false);

        string[] ShowOpenFileDialog(bool allowMultipleChoice);
    }
}
