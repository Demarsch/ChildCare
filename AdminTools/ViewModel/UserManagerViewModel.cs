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

        public UserManagerViewModel(PermissionsTreeViewModel permissionsTreeViewModel, UserEditorViewModel userEditorViewModel)
        {
            PermissionsTreeViewModel = permissionsTreeViewModel;
            UserEditorViewModel = userEditorViewModel;
        }  

    }
}
