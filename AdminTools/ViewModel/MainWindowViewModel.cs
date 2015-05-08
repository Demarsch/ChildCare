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

        private UserEditorViewModel userEditorViewModel;
        private PermissionsTreeViewModel permissionsTreeViewModel;

        public MainWindowViewModel(UserEditorViewModel userEditorViewModel, PermissionsTreeViewModel permissionsTreeViewModel)
        {
            this.userEditorViewModel = userEditorViewModel;
            this.permissionsTreeViewModel = permissionsTreeViewModel;
            
            this.UsersEditorCommand = new RelayCommand(this.UsersEditor);
            this.PermissionsTreeCommand = new RelayCommand(this.PermissionsTree);      
        }

        public void UsersEditor()
        {
            (new UserEditorView() { DataContext = userEditorViewModel }).ShowDialog();
        }

        public void PermissionsTree()
        {
            (new PermissionsTreeView() { DataContext = permissionsTreeViewModel }).ShowDialog();
        }
    }
}
