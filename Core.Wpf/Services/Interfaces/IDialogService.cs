using Core.Wpf.Mvvm;

namespace Core.Wpf.Services
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
