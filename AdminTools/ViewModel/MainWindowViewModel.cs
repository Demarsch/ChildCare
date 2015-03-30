using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminTools
{
    class MainWindowViewModel
    {
        public MainWindowViewModel(PermissionTreeViewModel permissionTreeViewModel)
        {
            if (permissionTreeViewModel == null)
                throw new ArgumentNullException("permissionTreeViewModel");
            PermissionTreeViewModel = permissionTreeViewModel;
        }

        public PermissionTreeViewModel PermissionTreeViewModel { get; private set; }
    }
}
