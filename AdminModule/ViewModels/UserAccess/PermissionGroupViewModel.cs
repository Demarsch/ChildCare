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
        private readonly PermissionGroup group;

        public PermissionGroupViewModel(PermissionGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            this.group = group;
            Name = group.Name;
            Description = group.Description;
            requestCurrentUserIncludeCommand = new DelegateCommand(RequestCurrentUserInclude, CanRequestCurrentUserInclude)
                .ObservesProperty(() => IsInUserMode)
                .ObservesProperty(() => CurrentUserIsIncluded);
            requestCurrentUserExcludeCommand = new DelegateCommand(RequestCurrentUserExclude, CanRequestCurrentUserExclude)
                .ObservesProperty(() => CurrentUserIsIncluded);
            requestDeleteCommand = new DelegateCommand(RequestDelete);
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
                if (SetProperty(ref userMode, value))
                {
                    OnPropertyChanged(() => IsInUserMode);
                    OnPropertyChanged(() => CurrentUserIsIncluded);
                }
            }
        }

        public bool IsInUserMode { get { return userMode != null; } }

        public bool CurrentUserIsIncluded { get { return userMode != null && group.UserPermisionGroups.Any(x => x.UserId == userMode.Id); } }

        private readonly DelegateCommand requestCurrentUserIncludeCommand;

        public ICommand RequestCurrentUserIncludeCommand { get { return requestCurrentUserIncludeCommand; } }

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
        
        private readonly DelegateCommand requestCurrentUserExcludeCommand;

        public ICommand RequestCurrentUserExcludeCommand { get { return requestCurrentUserExcludeCommand; } }

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

        private readonly DelegateCommand requestDeleteCommand;

        public ICommand RequestDeleteCommand { get { return requestDeleteCommand; } }

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
    }
}
