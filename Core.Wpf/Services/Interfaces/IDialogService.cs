using Core.Wpf.Mvvm;

namespace Core.Wpf.Services
{
    public interface IDialogService
    {
        bool? ShowDialog(IDialogViewModel dialogViewModel);

        void ShowError(string error);

        void ShowInformation(string message);

        void ShowWarning(string message);

        bool? AskUser(string question, bool isWarning = false);

        string[] ShowOpenFileDialog(bool allowMultipleChoice);
    }
}
