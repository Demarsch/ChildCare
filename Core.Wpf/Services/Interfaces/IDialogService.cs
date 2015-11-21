using Core.Wpf.Mvvm;

namespace Core.Wpf.Services
{
    public interface IDialogService
    {
        bool? ShowDialog(IDialogViewModel dialogViewModel);

        void ShowError(string error);

        bool? AskUser(string question, bool isWarning = false);

        string[] ShowOpenFileDialog(bool allowMultipleChoice);
    }
}
