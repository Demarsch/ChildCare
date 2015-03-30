using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Core;

namespace AdminTools
{
    /// <summary>
    /// This is the view-model of the UI.  It provides a data source
    /// for the TreeView (the FirstGeneration property), a bindable
    /// SearchText property, and the SearchCommand to perform a search.
    /// </summary>
    public class PermissionTreeViewModel
    {
        #region Data

        readonly ReadOnlyCollection<PermissionViewModel> _firstGeneration;
        readonly PermissionViewModel _rootPermission;
        readonly ICommand _searchCommand;
        private readonly IPermissionService permissionService;

        IEnumerator<PermissionViewModel> _matchingPermissionsEnumerator;
        string _searchText = String.Empty;

        #endregion // Data

        #region Constructor

        public PermissionTreeViewModel(IPermissionService permissionService)
        {
            if (permissionService == null)
                throw new ArgumentNullException("permissionService");
            this.permissionService = permissionService;

            _rootPermission = new PermissionViewModel(permissionService.GetRootPermissions().First());

            _firstGeneration = new ReadOnlyCollection<PermissionViewModel>(
                new PermissionViewModel[] 
                { 
                    _rootPermission 
                });

            _searchCommand = new SearchPermissionTreeCommand(this);
        }

        #endregion // Constructor

        #region Properties

        #region FirstGeneration

        /// <summary>
        /// Returns a read-only collection containing the first permission 
        /// in the tree, to which the TreeView can bind.
        /// </summary>
        public ReadOnlyCollection<PermissionViewModel> FirstGeneration
        {
            get { return _firstGeneration; }
        }

        #endregion // FirstGeneration

        #region SearchCommand

        /// <summary>
        /// Returns the command used to execute a search in the tree.
        /// </summary>
        public ICommand SearchCommand
        {
            get { return _searchCommand; }
        }

        private class SearchPermissionTreeCommand : ICommand
        {
            readonly PermissionTreeViewModel _permissionTree;

            public SearchPermissionTreeCommand(PermissionTreeViewModel permissionTree)
            {
                _permissionTree = permissionTree;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            event EventHandler ICommand.CanExecuteChanged
            {               
                add { }
                remove { }
            }

            public void Execute(object parameter)
            {
                _permissionTree.PerformSearch();
            }
        }

        #endregion // SearchCommand

        #region SearchText

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText)
                    return;

                _searchText = value;

                _matchingPermissionsEnumerator = null;
            }
        }

        #endregion // SearchText

        #endregion // Properties

        #region Search Logic

        void PerformSearch()
        {
            if (_matchingPermissionsEnumerator == null || !_matchingPermissionsEnumerator.MoveNext())
                this.VerifyMatchingPermissionsEnumerator();

            var permission = _matchingPermissionsEnumerator.Current;

            if (permission == null)
                return;

            // Ensure that this person is in view.
            if (permission.Parent != null)
                permission.Parent.IsExpanded = true;

            permission.IsSelected = true;
        }

        void VerifyMatchingPermissionsEnumerator()
        {
            var matches = this.FindMatches(_searchText, _rootPermission);
            _matchingPermissionsEnumerator = matches.GetEnumerator();

            if (!_matchingPermissionsEnumerator.MoveNext())
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

        #endregion // Search Logic
    }
}