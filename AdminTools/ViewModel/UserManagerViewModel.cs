using AdminTools.View;
using Core;
using DataLib;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace AdminTools.ViewModel
{
    class UserManagerViewModel : ObservableObject
    {
        public PermissionsTreeViewModel PermissionsTreeViewModel { get; private set; }
        public UserEditorViewModel UserEditorViewModel { get; private set; }

        public UserManagerViewModel(ISimpleLocator service)
        {
            var permissionsTreeViewModel = new PermissionsTreeViewModel(service);
            var permissionsTreeView = new PermissionsTreeView() { DataContext = permissionsTreeViewModel };
            PermissionsTreeViewModel = permissionsTreeViewModel;

            var usersEditorViewModel = new UserEditorViewModel(service);
            var usersEditorView = new UserEditorView() { DataContext = usersEditorViewModel };
            UserEditorViewModel = usersEditorViewModel;
        }  

    }
}
