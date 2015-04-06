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
using DataLib;

namespace AdminTools.ViewModel
{
    public class PermissionViewModel :  INotifyPropertyChanged
    {
        #region Data

        readonly ObservableCollection<PermissionViewModel> children;
        readonly PermissionViewModel parent;
        readonly Permission permission;
        readonly IPermissionService permissionService;

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
                        permissionService.GetChildren(currentPermission).Select(x => new PermissionViewModel(this.permissionService, x, this)).ToList<PermissionViewModel>());
        }

        #endregion // Constructors

        #region Person Properties

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

        #region Presentation Members

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (isExpanded && parent != null)
                    parent.IsExpanded = true;
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion 

        #region NameContainsText

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.Name))
                return false;

            return this.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        #endregion
        
        #endregion // Presentation Members        

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}