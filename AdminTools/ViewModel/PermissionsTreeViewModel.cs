using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using DataLib;
using GalaSoft.MvvmLight;
using Core;
using AdminTools.View;
using log4net;

namespace AdminTools.ViewModel
{
    public class PermissionsTreeViewModel : ObservableObject
    {
        #region PermissionData

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

        private IEnumerator<PermissionViewModel> matchingPermissionsEnumerator;
        private string searchText = String.Empty;

        #endregion 
             
        #region Constructor

        private IPermissionService permissionService;
        private ILog log;

        public PermissionsTreeViewModel(IPermissionService permissionService, ILog log)
        {
            this.permissionService = permissionService;
            this.log = log;

            permissionRoots = new ObservableCollection<PermissionViewModel>();
            foreach (var item in permissionService.GetRootPermissions())
                permissionRoots.Add(new PermissionViewModel(permissionService, item, null));	        

            this.SearchPermissionCommand = new RelayCommand(this.SearchPermission);
            this.DeletePermissionCommand = new RelayCommand<object>(DeletePermission);
            this.EditPermissionCommand = new RelayCommand<object>(EditPermission);
            this.CreatePermissionCommand = new RelayCommand<object>(CreatePermission);
        }         

        #endregion 

        #region Properties

        #region PermissionRoots

        /// <summary>
        /// Returns a collection of root permissions in the tree, to which the TreeView can bind.
        /// </summary>       
        private ObservableCollection<PermissionViewModel> permissionRoots;
        public ObservableCollection<PermissionViewModel> PermissionRoots
        {
            get { return permissionRoots; }
            set { Set("PermissionRoots", ref permissionRoots, value); }
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

            var currentPermission = (parameter as PermissionViewModel);
            if (MessageBox.Show("Удалить право \"" + currentPermission.Name + "\" и все вложенные в него права ?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var permission = permissionService.GetPermissionById(currentPermission.Id);
                try
                {
                    this.permissionService.Delete(permission);
                
                    if (currentPermission.Parent == null)
                        PermissionRoots.Remove(currentPermission);
                    else
                        currentPermission.Parent.Children.Remove(currentPermission);
                    MessageBox.Show("Данные удалены");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("При сохранении возникла ошибка: " + ex.Message);
                    log.Error(string.Format("Failed to Delete permission. " + ex.Message));
                }                
            }
        }

        private void EditPermission(object parameter)
        {
            if (parameter == null || !(parameter is PermissionViewModel))
                return;

            var currentPermission = (parameter as PermissionViewModel);
            var editPermissionViewModel = new EditPermissionViewModel(permissionService, log, currentPermission.Parent, currentPermission);
            EditPermissionView view = new EditPermissionView() { DataContext = editPermissionViewModel, Title = "Редактировать право" };
            view.ShowDialog();

            currentPermission.Name = editPermissionViewModel.CurrentPermission.Name;
            currentPermission.Description = editPermissionViewModel.CurrentPermission.Description;
            currentPermission.IsGroup = editPermissionViewModel.CurrentPermission.IsGroup;
            currentPermission.ReadOnly = editPermissionViewModel.CurrentPermission.ReadOnly;
        }

        private void CreatePermission(object parameter)
        {
            var parent = (parameter as PermissionViewModel);
            var editPermissionViewModel = new EditPermissionViewModel(permissionService, log, parent, null);
            EditPermissionView view = new EditPermissionView() { DataContext = editPermissionViewModel, Title = "Новое право" };
            view.ShowDialog();

            if (editPermissionViewModel.CurrentPermission == null) return;

            if (parent == null)
                PermissionRoots.Add(editPermissionViewModel.CurrentPermission);
            else
            {
                editPermissionViewModel.CurrentPermission.Parent = parent;
                parent.Children.Add(editPermissionViewModel.CurrentPermission);
            }
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