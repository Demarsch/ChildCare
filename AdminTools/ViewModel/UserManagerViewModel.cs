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
        private readonly ILog log;

        public UserManagerViewModel(ILog log)
        {                                  
            var mainService = new MainServiceLocator();
            this.log = log;
            var permissionsTreeViewModel = new PermissionsTreeViewModel(mainService);
            var permissionsTreeView = new PermissionsTreeView() { DataContext = permissionsTreeViewModel };
            PermissionsTreeViewModel = permissionsTreeViewModel;

            var usersEditorViewModel = new UserEditorViewModel(mainService, this.log);
            var usersEditorView = new UserEditorView() { DataContext = usersEditorViewModel };
            UserEditorViewModel = usersEditorViewModel;
        }  

    }
}
