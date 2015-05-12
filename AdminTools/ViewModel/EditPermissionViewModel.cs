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
using Core;
using log4net;
using GalaSoft.MvvmLight;
using MainLib;

namespace AdminTools.ViewModel
{
    class EditPermissionViewModel : ObservableObject
    {
        private IPermissionService permissionService;
        private ILog log;
        private PermissionViewModel parent;
               
        public EditPermissionViewModel(IPermissionService permissionService, ILog log, PermissionViewModel parent, PermissionViewModel permission)
        {            
            this.permissionService = permissionService;
            this.log = log;
            this.SavePermissionCommand = new RelayCommand(SavePermission);

            this.parent = parent;
            this.currentPermission = permission;  
            if (this.currentPermission != null)
                LoadPermission();
        }
               
        private RelayCommand savePermissionCommand;
        public RelayCommand SavePermissionCommand
        {
            get { return savePermissionCommand; }
            set { Set("SavePermissionCommand", ref savePermissionCommand, value); }
        }

        private PermissionViewModel currentPermission;
        public PermissionViewModel CurrentPermission
        {
            get { return currentPermission; }
            set { Set("CurrentPermission", ref currentPermission, value); }
        }        

        private string permissionName;
        public string PermissionName
        {
            get { return permissionName; }
            set { Set("PermissionName", ref permissionName, value); }
        }

        private string permissionDescription;
        public string PermissionDescription
        {
            get { return permissionDescription; }
            set { Set("PermissionDescription", ref permissionDescription, value); }
        }

        private bool isPermissionGroup;
        public bool IsPermissionGroup
        {
            get { return isPermissionGroup; }
            set { Set("IsPermissionGroup", ref isPermissionGroup, value); }
        }

        private bool isPermissionReadOnly;
        public bool IsPermissionReadOnly
        {
            get { return isPermissionReadOnly; }
            set { Set("IsPermissionReadOnly", ref isPermissionReadOnly, value); }
        }

        private void LoadPermission()
        {
            PermissionName = CurrentPermission.Name;
            PermissionDescription = CurrentPermission.Description;
            IsPermissionGroup = CurrentPermission.IsGroup;
            IsPermissionReadOnly = CurrentPermission.ReadOnly;
        }

        private void SavePermission()
        {
            if (PermissionName.HasNoData())
            {
                MessageBox.Show("Укажите название права.");
                return;
            }

            var parentName = (this.parent != null ? this.parent.Name : "Корень дерева");

            Permission permission = null;
            if (this.CurrentPermission == null && MessageBox.Show("Добавить новый узел к элементу \"" + parentName + "\" ?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                permission = new Permission();
            else if (this.CurrentPermission != null)
                permission = permissionService.GetPermissionById(this.CurrentPermission.Id);
            else
                return;

            permission.Name = PermissionName;
            permission.Description = PermissionDescription.ToSafeString();
            permission.IsGroup = IsPermissionGroup;
            permission.ReadOnly = IsPermissionReadOnly;
            
            try
            {
                permission.Id = permissionService.Save(permission, parent != null ? parent.Id : (int?)null);
                this.CurrentPermission = new PermissionViewModel(permissionService, permission, parent);
                MessageBox.Show("Данные сохранены");
            }
            catch(Exception ex)
            {
                MessageBox.Show("При сохранении возникла ошибка: " + ex.Message);
                log.Error(string.Format("Failed to Save permission. " + ex.Message));
            }            
        }
    }
}
