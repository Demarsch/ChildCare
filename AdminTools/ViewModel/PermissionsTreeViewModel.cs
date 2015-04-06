using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using DataLib;

namespace AdminTools.ViewModel
{    
    public class PermissionsTreeViewModel : FailableViewModel
    {
        #region PermissionData

        private ObservableCollection<PermissionViewModel> firstGeneration;
        private PermissionViewModel rootPermission;
        public RelayCommand SearchPermissionCommand { get; private set; }
       
        private readonly IPermissionService permissionService;

        private IEnumerator<PermissionViewModel> matchingPermissionsEnumerator;
        private string searchText = String.Empty;

        #endregion 
             
        #region Constructor

        public PermissionsTreeViewModel(IPermissionService permissionService)
        {
            if (permissionService == null)
                throw new ArgumentNullException("permissionService");
            this.permissionService = permissionService;
                     
            rootPermission = new PermissionViewModel(permissionService, permissionService.GetRootPermission());
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