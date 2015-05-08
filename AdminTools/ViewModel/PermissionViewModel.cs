using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using DataLib;
using GalaSoft.MvvmLight;
using Core;

namespace AdminTools.ViewModel
{
    public class PermissionViewModel : ObservableObject
    {
        #region Data

        private ObservableCollection<PermissionViewModel> children;
        private PermissionViewModel parent;
        private Permission permission;
        private IPermissionService permissionService;

        bool isExpanded;
        bool isSelected;

        #endregion // Data

        #region Constructors

        public PermissionViewModel(IPermissionService permissionService, Permission currentPermission)
            : this(permissionService, currentPermission, null)
        {
        }

        private PermissionViewModel(IPermissionService permissionService, Permission currentPermission, PermissionViewModel parent)
        {
            this.permissionService = permissionService;
            this.permission = currentPermission;
            this.parent = parent;

            children = new ObservableCollection<PermissionViewModel>(
                       permissionService.GetChildren(currentPermission.Id).Select(x => new PermissionViewModel(permissionService, x, this)));
        }

        #endregion // Constructors

        #region Permission Properties

        public ObservableCollection<PermissionViewModel> Children
        {
            get { return children; }
        }

        public string Name
        {
            get { return permission.Name; }
        }
           
        public bool IsGroup
        {
            get { return permission.IsGroup; }
        }

        public string PhotoSource
        {
            get
            {
                return IsGroup
                    ? "pack://application:,,,/Resources;component/Images/UserGroup48x48.png"
                    : string.Empty;
            }
        }

        public PermissionViewModel Parent
        {
            get { return parent; }
        }

        #endregion
                
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    Set("IsExpanded", ref isExpanded, value);
                }

                // Expand all the way up to the root.
                if (isExpanded && parent != null)
                    parent.IsExpanded = true;
            }
        }
        
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    Set("IsSelected", ref isSelected, value); 
                }
            }
        }        

        #region NameContainsText

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.Name))
                return false;

            return this.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        #endregion            
       
    }
}