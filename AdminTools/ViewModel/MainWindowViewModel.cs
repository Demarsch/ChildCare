using GalaSoft.MvvmLight.CommandWpf;
using AdminTools.View;
using GalaSoft.MvvmLight;
using Core;
using log4net;

namespace AdminTools.ViewModel
{
    class MainWindowViewModel : ObservableObject
    {
        public RelayCommand UsersEditorCommand { get; private set; }
        public RelayCommand PermissionsTreeCommand { get; private set; }
        public RelayCommand ScanDocumentsCommand { get; private set; }

        private UserEditorViewModel userEditorViewModel;
        private PermissionsTreeViewModel permissionsTreeViewModel;
        private ScanDocumentsViewModel scanDocumentsViewModel;

        public MainWindowViewModel(UserEditorViewModel userEditorViewModel, PermissionsTreeViewModel permissionsTreeViewModel, ScanDocumentsViewModel scanDocumentsViewModel)
        {
            this.userEditorViewModel = userEditorViewModel;
            this.permissionsTreeViewModel = permissionsTreeViewModel;
            this.scanDocumentsViewModel = scanDocumentsViewModel;
            
            this.UsersEditorCommand = new RelayCommand(this.UsersEditor);
            this.PermissionsTreeCommand = new RelayCommand(this.PermissionsTree);
            this.ScanDocumentsCommand = new RelayCommand(this.ScanDocuments);   
        }

        public void UsersEditor()
        {
            (new UserEditorView() { DataContext = userEditorViewModel }).ShowDialog();
        }

        public void PermissionsTree()
        {
            (new PermissionsTreeView() { DataContext = permissionsTreeViewModel }).ShowDialog();
        }

        public void ScanDocuments()
        {
            (new ScanDocumentsView() { DataContext = scanDocumentsViewModel }).ShowDialog();
        }
    }
}
