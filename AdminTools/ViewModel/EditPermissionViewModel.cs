﻿using System;
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
        private readonly ISimpleLocator service;
        private PermissionViewModel parent;

        public EditPermissionViewModel(ISimpleLocator service, PermissionViewModel parent)
        {            
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.parent = parent;
            this.SavePermissionCommand = new RelayCommand(SavePermission);
        }

        public EditPermissionViewModel(ISimpleLocator service, PermissionViewModel parent, PermissionViewModel permission)
            : this(service, parent)
        {
            this.CurrentPermission = permission;
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
            {
                permission = new Permission();
                if (parent == null)
                    permission.PermissionLinks1.Add(new PermissionLink());
                else
                    permission.PermissionLinks1.Add(new PermissionLink() { ParentId = parent.Id });
            }
            else if (this.CurrentPermission != null)
                permission = this.service.Instance<IPermissionService>().GetPermissionById(this.CurrentPermission.Id);
            else
                return;

            permission.Name = PermissionName;
            permission.Description = PermissionDescription.ToSafeString();
            permission.IsGroup = IsPermissionGroup;
            permission.ReadOnly = IsPermissionReadOnly;

            string message = string.Empty;
            if (this.service.Instance<IPermissionService>().Save(permission, out message))
            {
                this.CurrentPermission = new PermissionViewModel(this.service, permission);
                MessageBox.Show("Данные сохранены");
            }
            else
            {
                MessageBox.Show("При сохранении возникла ошибка: " + message);
                this.service.Instance<ILog>().Error(string.Format("Failed to Save permission. " + message));
            }
        }
    }
}
