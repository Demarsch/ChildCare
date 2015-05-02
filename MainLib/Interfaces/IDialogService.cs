namespace Core
{
    public interface IDialogService
    {
        bool? ShowDialog(IDialogViewModel dialogViewModel);

        void ShowError(string errorMessage);
    }
}
