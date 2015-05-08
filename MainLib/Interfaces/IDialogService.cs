namespace Core
{
    public interface IDialogService
    {
        bool? ShowDialog(IDialogViewModel dialogViewModel);

        void ShowError(string errorMessage);

        bool? AskUser(string question, bool isWarning = false);
    }
}
