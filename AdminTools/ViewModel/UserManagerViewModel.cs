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
    class UserManagerViewModel
    {
        public PermissionsTreeViewModel PermissionsTreeViewModel { get; private set; }
        public UserEditorViewModel UserEditorViewModel { get; private set; }

        public UserManagerViewModel()
        {
            var contextProvider = new ModelContextProvider();

            var permissionService = new PermissionService(contextProvider) as IPermissionService;
            var userService = new UserService(contextProvider) as IUserService;

            var permissionsTreeViewModel = new PermissionsTreeViewModel(permissionService);
            var permissionsTreeView = new PermissionsTreeView() { DataContext = permissionsTreeViewModel };
            PermissionsTreeViewModel = permissionsTreeViewModel;

            var usersEditorViewModel = new UserEditorViewModel(userService);
            var usersEditorView = new UserEditorView() { DataContext = usersEditorViewModel };
            UserEditorViewModel = usersEditorViewModel;
        }  

    }
}
