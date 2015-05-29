namespace Core
{
    public interface IDialogService
    {
        bool? ShowDialog(IDialogViewModel dialogViewModel);

        void ShowError(string errorMessage);

        void ShowMessage(string message);

        bool? AskUser(string question, bool isWarning = false);
    }
}
