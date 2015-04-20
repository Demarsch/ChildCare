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

        readonly ObservableCollection<PermissionViewModel> children;
        readonly PermissionViewModel parent;
        readonly Permission permission;
        readonly ISimpleLocator service;

        bool isExpanded;
        bool isSelected;

        #endregion // Data

        #region Constructors

        public PermissionViewModel(ISimpleLocator service, Permission currentPermission)
            : this(service, currentPermission, null)
        {
        }

        private PermissionViewModel(ISimpleLocator service, Permission currentPermission, PermissionViewModel parent)
        {
            this.service = service;
            this.permission = currentPermission;
            this.parent = parent;

            children = new ObservableCollection<PermissionViewModel>(
                       service.Instance<IPermissionService>().GetChildren(currentPermission.Id).Select(x => new PermissionViewModel(this.service, x, this)));
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