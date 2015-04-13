using AdminTools.View;
using Core;
using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminTools.ViewModel
{
    class UserManagerViewModel : FailableViewModel
    {
        public PermissionsTreeViewModel PermissionsTreeViewModel { get; private set; }
        public UserEditorViewModel UserEditorViewModel { get; private set; }

        public UserManagerViewModel()
        {                                  
            var mainService = new MainServiceLocator();
            
            var permissionsTreeViewModel = new PermissionsTreeViewModel(mainService);
            var permissionsTreeView = new PermissionsTreeView() { DataContext = permissionsTreeViewModel };
            PermissionsTreeViewModel = permissionsTreeViewModel;

            var usersEditorViewModel = new UserEditorViewModel(mainService);
            var usersEditorView = new UserEditorView() { DataContext = usersEditorViewModel };
            UserEditorViewModel = usersEditorViewModel;
        }  

    }
}
