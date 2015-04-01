using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight.Command;

namespace AdminTools
{
    class MainWindowViewModel
    {
        public RelayCommand PermissionsEditorCommand { get; private set; }
        private readonly IPermissionService permissionService;

        public MainWindowViewModel(IPermissionService permissionService)
        {
            this.permissionService = permissionService;
            this.PermissionsEditorCommand = new RelayCommand(this.PermissionsEditor);
        }
        
        public void PermissionsEditor()
        {
            var permissionsEditorViewModel = new PermissionsEditorViewModel(permissionService);
            var permissionsEditorView = new PermissionsEditorView() { DataContext = permissionsEditorViewModel };

            permissionsEditorView.ShowDialog();

        }
    }
}
