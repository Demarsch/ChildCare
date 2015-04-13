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

        private ObservableCollection<PermissionViewModel> permissionRoots;
        public RelayCommand SearchPermissionCommand { get; private set; }
        
        private RelayCommand<object> createPermissionCommand;
        public RelayCommand<object> CreatePermissionCommand
        {
            get { return createPermissionCommand; }
            set
            {
                Set("CreatePermissionCommand", ref createPermissionCommand, value);
                
            }
        }

        private RelayCommand<object> editPermissionCommand;
        public RelayCommand<object> EditPermissionCommand
        {
            get { return editPermissionCommand; }
            set
            {
                Set("EditPermissionCommand", ref editPermissionCommand, value);                
            }
        }

        private RelayCommand<object> deletePermissionCommand;
        public RelayCommand<object> DeletePermissionCommand
        {
            get { return deletePermissionCommand; }
            set
            {
                Set("DeletePermissionCommand", ref deletePermissionCommand, value);
            }
        }

        private readonly ISimpleLocator service;

        private IEnumerator<PermissionViewModel> matchingPermissionsEnumerator;
        private string searchText = String.Empty;

        #endregion 
             
        #region Constructor

        public PermissionsTreeViewModel(ISimpleLocator service)
        {
            if (service == null)
                throw new ArgumentNullException("permissionService");
            this.service = service;
            
            permissionRoots = new ObservableCollection<PermissionViewModel>();
            foreach (var item in service.Instance<IPermissionService>().GetRootPermissions())
                permissionRoots.Add(new PermissionViewModel(service, item));	        

            this.SearchPermissionCommand = new RelayCommand(this.SearchPermission);
            this.DeletePermissionCommand = new RelayCommand<object>(DeletePermission);
            this.EditPermissionCommand = new RelayCommand<object>(EditPermission);
            this.CreatePermissionCommand = new RelayCommand<object>(CreatePermission);
        }         

        #endregion 

        #region Properties

        #region FirstGeneration

        /// <summary>
        /// Returns a collection containing the first permission 
        /// in the tree, to which the TreeView can bind.
        /// </summary>
        public ObservableCollection<PermissionViewModel> PermissionRoots
        {
            get { return permissionRoots; }
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
        
        #region Context Menu Command

        private void DeletePermission(object parameter)
        {
            if (parameter == null || !(parameter is PermissionViewModel))
                return;
            MessageBox.Show(
                        "Удаление права " + (parameter as PermissionViewModel).Name,
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
        }

        private void EditPermission(object parameter)
        {
            if (parameter == null || !(parameter is PermissionViewModel))
                return;
            MessageBox.Show(
                        "Редактирование права " + (parameter as PermissionViewModel).Name,
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
        }

        private void CreatePermission(object parameter)
        {
            if (parameter == null || !(parameter is PermissionViewModel))
                return;
            MessageBox.Show(
                        "Создание нового права. Родитель -> " + (parameter as PermissionViewModel).Name,
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                        );
        }

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
            var matches = this.FindMatches(searchText, permissionRoots.ToArray());
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

        IEnumerable<PermissionViewModel> FindMatches(string searchText, PermissionViewModel[] roots)
        {
            foreach (var permission in roots)
            {
                if (permission.NameContainsText(searchText))
                    yield return permission;

                foreach (PermissionViewModel child in permission.Children)
                    foreach (PermissionViewModel match in this.FindMatches(searchText, new PermissionViewModel[] { child }))
                        yield return match;
            }            
        }

        #endregion
    }
}