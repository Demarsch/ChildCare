using System;
using System.Linq;
using System.Windows.Input;
using Core.Data;
using Prism.Commands;
using Prism.Mvvm;

namespace AdminModule.ViewModels
{
    public class PermissionGroupViewModel : BindableBase
    {
        public PermissionGroupViewModel(PermissionGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            Group = group;
            RequestCurrentUserIncludeCommand = new DelegateCommand(RequestCurrentUserInclude, CanRequestCurrentUserInclude)
                .ObservesProperty(() => IsInUserMode)
                .ObservesProperty(() => CurrentUserIsIncluded);
            RequestCurrentUserExcludeCommand = new DelegateCommand(RequestCurrentUserExclude, CanRequestCurrentUserExclude)
                .ObservesProperty(() => CurrentUserIsIncluded);
            RequestDeleteCommand = new DelegateCommand(RequestDelete);
            RequestEditCommand = new DelegateCommand(RequestEdit);
        }

        private PermissionGroup group;

        public PermissionGroup Group
        {
            get { return group; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                group = value;
                Name = group.Name;
                Description = group.Description;
                OnPropertyChanged(string.Empty);
            }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        private UserViewModel userMode;

        public UserViewModel UserMode
        {
            get { return userMode; }
            set
            {
                SetProperty(ref userMode, value);
                OnPropertyChanged(() => IsInUserMode);
                OnPropertyChanged(() => CurrentUserIsIncluded);
            }
        }

        public bool IsInUserMode { get { return userMode != null; } }

        public bool CurrentUserIsIncluded { get { return userMode != null && group.UserPermisionGroups.Any(x => x.UserId == userMode.Id); } }

        public ICommand RequestCurrentUserIncludeCommand { get; private set; }

        private void RequestCurrentUserInclude()
        {
            OnCurrentUserIncludeRequested();
        }

        private bool CanRequestCurrentUserInclude()
        {
            return IsInUserMode && !CurrentUserIsIncluded;
        }

        public event EventHandler CurrentUserIncludeRequested;

        protected virtual void OnCurrentUserIncludeRequested()
        {
            var handler = CurrentUserIncludeRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public ICommand RequestCurrentUserExcludeCommand { get; private set; }

        private void RequestCurrentUserExclude()
        {
            OnCurrentUserExcludeRequested();
        }

        private bool CanRequestCurrentUserExclude()
        {
            return CurrentUserIsIncluded;
        }

        public event EventHandler CurrentUserExcludeRequested;

        protected virtual void OnCurrentUserExcludeRequested()
        {
            var handler = CurrentUserExcludeRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public ICommand RequestDeleteCommand { get; private set; }

        private void RequestDelete()
        {
            OnDeleteRequested();
        }

        public event EventHandler DeleteRequested;

        protected virtual void OnDeleteRequested()
        {
            var handler = DeleteRequested;
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
