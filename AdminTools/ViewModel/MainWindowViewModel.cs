using GalaSoft.MvvmLight.CommandWpf;
using AdminTools.View;
using GalaSoft.MvvmLight;
using Core;

namespace AdminTools.ViewModel
{
    class MainWindowViewModel : ObservableObject
    {
        public RelayCommand UsersEditorCommand { get; private set; }
        public RelayCommand PermissionsTreeCommand { get; private set; }
        private ISimpleLocator service;

        public MainWindowViewModel(ISimpleLocator service)
        {
            this.service = service;
            this.UsersEditorCommand = new RelayCommand(this.UsersEditor);
            this.PermissionsTreeCommand = new RelayCommand(this.PermissionsTree);      
        }

        public void UsersEditor()
        {
            (new UserEditorView() { DataContext = new UserEditorViewModel(this.service) }).ShowDialog();
        }

        public void PermissionsTree()
        {
            (new PermissionsTreeView() { DataContext = new PermissionsTreeViewModel(this.service) }).ShowDialog();
        }
    }
}
