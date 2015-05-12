using System;
using System.Collections.ObjectModel;
using System.Linq;
using DataLib;
using GalaSoft.MvvmLight;
using Core;

namespace AdminTools.ViewModel
{
    public class PermissionViewModel : ObservableObject
    {
        #region Data

        private Permission permission;
        private IPermissionService permissionService;

        bool isExpanded;
        bool isSelected;

        #endregion // Data

        #region Constructors
                
        public PermissionViewModel(IPermissionService permissionService, Permission currentPermission, PermissionViewModel parent)
        {
            this.permissionService = permissionService;
            this.permission = currentPermission;
            this.parent = parent;

            name = this.permission.Name;
            description = this.permission.Description;
            isGroup = this.permission.IsGroup;
            readOnly = this.permission.ReadOnly;
            id = this.permission.Id;

            children = new ObservableCollection<PermissionViewModel>(
                       permissionService.GetChildren(currentPermission.Id).Select(x => new PermissionViewModel(permissionService, x, this)));
        }

        #endregion // Constructors

        #region Permission Properties
       
        private ObservableCollection<PermissionViewModel> children;
        public ObservableCollection<PermissionViewModel> Children
        {
            get { return children; }
            set { Set("Children", ref children, value); }
        }

        private int id;
        public int Id
        {
            get { return permission.Id; }
            set { Set("Id", ref id, value); }
        }
        
        private string name;
        public string Name
        {
            get { return name; }
            set { Set("Name", ref name, value); }
        }
        
        private string description;
        public string Description
        {
            get { return description; }
            set { Set("Description", ref description, value); }
        }
            
        private bool isGroup;
        public bool IsGroup
        {
            get { return isGroup; }
            set 
            { 
                if (!Set("IsGroup", ref isGroup, value))
                    return;
                PhotoSource = (value ? "pack://application:,,,/Resources;component/Images/UserGroup48x48.png" : string.Empty);
            }
        }
               
        private bool readOnly;
        public bool ReadOnly
        {
            get { return readOnly; }
            set { Set("ReadOnly", ref readOnly, value); }
        }

        private string photoSource;
        public string PhotoSource
        {
            get
            {
                return IsGroup
                    ? "pack://application:,,,/Resources;component/Images/UserGroup48x48.png"
                    : string.Empty;
            }
            set { Set("PhotoSource", ref photoSource, value); }
        }       

        private PermissionViewModel parent;
        public PermissionViewModel Parent
        {
            get { return parent; }
            set { Set("Parent", ref parent, value); }
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