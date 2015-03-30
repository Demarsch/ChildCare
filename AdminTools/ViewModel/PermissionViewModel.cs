using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DataLib;

namespace AdminTools
{
    /// <summary>
    /// A UI-friendly wrapper around a Person object.
    /// </summary>
    public class PermissionViewModel :  INotifyPropertyChanged
    {
        #region Data

        readonly ReadOnlyCollection<PermissionViewModel> _children;
        readonly PermissionViewModel _parent;
        readonly Permission _permission;

        bool _isExpanded;
        bool _isSelected;

        #endregion // Data

        #region Constructors

        public PermissionViewModel(Permission permission)
            : this(permission, null)
        {
        }

        private PermissionViewModel(Permission permission, PermissionViewModel parent)
        {
            _permission = permission;
            _parent = parent;

            _children = new ReadOnlyCollection<PermissionViewModel>(
                    _permission.PermissionLinks.Select(x => new PermissionViewModel(x.Permission1, this))
                     .ToList<PermissionViewModel>());
        }

        #endregion // Constructors

        #region Person Properties

        public ReadOnlyCollection<PermissionViewModel> Children
        {
            get { return _children; }
        }

        public string Name
        {
            get { return _permission.Name; }
        }
           
        public bool IsGroup
        {
            get { return _permission.IsGroup; }
        }

        public string PhotoSource
        {
            get
            {
                return IsGroup
                    ? "pack://application:,,,/Resources;component/Images/Man48x48.png"
                    : string.Empty;
            }
        }

        public PermissionViewModel Parent
        {
            get { return _parent; }
        }

        #endregion // Person Properties

        #region Presentation Members

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected

        #region NameContainsText

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.Name))
                return false;

            return this.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        #endregion // NameContainsText
        
        #endregion // Presentation Members        

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}