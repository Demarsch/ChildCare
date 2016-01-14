﻿using System;
using System.Linq;
using System.Windows.Input;
using AdminModule.Model;
using Core.Data;
using Core.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace AdminModule.ViewModels
{
    public class UserViewModel : BindableBase
    {
        public UserViewModel(UserDTO user, ICacheService cacheService)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            User = cacheService.GetItemById<User>(user.Id);
            Id = user.Id;
            FullName = user.FullName;
            IsActive = user.ActiveFrom.Date <= DateTime.Today && DateTime.Today <= user.ActiveTo.Date;
            RequestIncludeInCurrentGroupCommand = new DelegateCommand(RequestIncludeInCurrentGroup, CanRequestIncludeInCurrentGroup)
                .ObservesProperty(() => IsInGroupMode)
                .ObservesProperty(() => IsIncludedInCurrentGroup);
            RequestExcludeFromCurrentGroupCommand = new DelegateCommand(RequestExcludeFromCurrentGroup, CanRequestExcludeFromCurrentGroup)
                .ObservesProperty(() => IsIncludedInCurrentGroup);
            RequestActivationChangeCommand = new DelegateCommand(RequestActivationChange);
            RequestEditCommand = new DelegateCommand(RequestEdit);
        }

        public User User { get; private set; }

        private string fullName;

        public string FullName
        {
            get { return fullName; }
            set { SetProperty(ref fullName, value); }
        }

        public int Id { get; private set; }
        
        private PermissionGroupViewModel groupMode;

        public PermissionGroupViewModel GroupMode
        {
            get { return groupMode; }
            set
            {
                SetProperty(ref groupMode, value);
                OnPropertyChanged(() => IsInGroupMode);
                OnPropertyChanged(() => IsIncludedInCurrentGroup);
            }
        }

        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            set { SetProperty(ref isActive, value); }
        }

        public bool IsInGroupMode { get { return groupMode != null; } }

        public bool IsIncludedInCurrentGroup { get { return groupMode != null && User.UserPermissionGroups.Any(x => x.PermissionGroupId == groupMode.Group.Id); } }

        public ICommand RequestIncludeInCurrentGroupCommand { get; private set; }

        private void RequestIncludeInCurrentGroup()
        {
            OnIncludeInCurrentGroupRequested();
        }

        private bool CanRequestIncludeInCurrentGroup()
        {
            return IsInGroupMode && !IsIncludedInCurrentGroup;
        }

        public event EventHandler IncludeIntoCurrentGroupRequested;

        protected virtual void OnIncludeInCurrentGroupRequested()
        {
            var handler = IncludeIntoCurrentGroupRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public ICommand RequestExcludeFromCurrentGroupCommand { get; private set; }

        private void RequestExcludeFromCurrentGroup()
        {
            OnExcludeFromCurrentGroupRequested();
        }

        private bool CanRequestExcludeFromCurrentGroup()
        {
            return IsIncludedInCurrentGroup;
        }

        public event EventHandler ExcludeFromCurrentGroupRequested;

        protected virtual void OnExcludeFromCurrentGroupRequested()
        {
            var handler = ExcludeFromCurrentGroupRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public ICommand RequestActivationChangeCommand { get; private set; }

        private void RequestActivationChange()
        {
            OnActivationChangeRequested();
        }

        public event EventHandler ActivationChangeRequested;

        protected virtual void OnActivationChangeRequested()
        {
            var handler = ActivationChangeRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public ICommand RequestEditCommand { get; private set; }

        private void RequestEdit()
        {
            OnEditRequested();
        }

        public event EventHandler EditRequested;

        protected virtual void OnEditRequested()
        {
            var handler = EditRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}