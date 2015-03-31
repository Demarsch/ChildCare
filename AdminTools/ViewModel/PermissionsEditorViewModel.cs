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
using System.Windows;

namespace AdminTools
{
    /// <summary>
    /// This is the view-model of the UI.  It provides a data source
    /// for the PermissionsEditorView (the FirstGeneration property), a bindable
    /// SearchText property, and the SearchCommand to perform a search.
    /// </summary>
    public class PermissionsEditorViewModel
    {
        #region Data

        readonly ObservableCollection<PermissionViewModel> firstGeneration;
        readonly PermissionViewModel rootPermission;
        public RelayCommand SearchPermissionCommand { get; private set; }
       
        private readonly IPermissionService permissionService;

        IEnumerator<PermissionViewModel> matchingPermissionsEnumerator;
        string searchText = String.Empty;

        #endregion 

        #region Constructor

        public PermissionsEditorViewModel(IPermissionService permissionService)
        {
            if (permissionService == null)
                throw new ArgumentNullException("permissionService");
            this.permissionService = permissionService;

            rootPermission = new PermissionViewModel(permissionService.GetRootPermissions().First());

            firstGeneration = new ObservableCollection<PermissionViewModel>(
                new PermissionViewModel[] 
                { 
                    rootPermission 
                });

            this.SearchPermissionCommand = new RelayCommand(this.SearchPermission);
        }               

        #endregion 

        #region Properties

        #region FirstGeneration

        /// <summary>
        /// Returns a collection containing the first permission 
        /// in the tree, to which the TreeView can bind.
        /// </summary>
        public ObservableCollection<PermissionViewModel> FirstGeneration
        {
            get { return firstGeneration; }
        }

        #endregion         

        #region SearchText

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (value == searchText)
                    return;

                searchText = value;

                matchingPermissionsEnumerator = null;
            }
        }

        #endregion

        #endregion

        #region Search Logic

        private void SearchPermission()
        {
            if (matchingPermissionsEnumerator == null || !matchingPermissionsEnumerator.MoveNext())
                this.VerifyMatchingPermissionsEnumerator();

            var permission = matchingPermissionsEnumerator.Current;

            if (permission == null)
                return;

            if (permission.Parent != null)
                permission.Parent.IsExpanded = true;

            permission.IsSelected = true;
        }

        void VerifyMatchingPermissionsEnumerator()
        {
            var matches = this.FindMatches(searchText, rootPermission);
            matchingPermissionsEnumerator = matches.GetEnumerator();

            if (!matchingPermissionsEnumerator.MoveNext())
            {
                MessageBox.Show(
                    "Не найдено ни одного совпадения.",
                    "Попробуйте снова",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
            }
        }

        IEnumerable<PermissionViewModel> FindMatches(string searchText, PermissionViewModel permission)
        {
            if (permission.NameContainsText(searchText))
                yield return permission;

            foreach (PermissionViewModel child in permission.Children)
                foreach (PermissionViewModel match in this.FindMatches(searchText, child))
                    yield return match;
        }

        #endregion
    }
}