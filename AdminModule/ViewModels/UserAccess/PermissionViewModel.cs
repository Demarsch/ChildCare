using System;
using System.Linq;
using System.Windows.Input;
using Core.Data;
using Prism.Commands;
using Prism.Mvvm;

namespace AdminModule.ViewModels
{
    public class PermissionViewModel : BindableBase
    {
        public PermissionViewModel(Permission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
            Permission = permission;
            RequestIncludeInCurrentGroupCommand = new DelegateCommand(RequestIncludeInCurrentGroup, CanRequestIncludeInCurrentGroup)
                .ObservesProperty(() => IsInGroupMode)
                .ObservesProperty(() => IsIncludedInCurrentGroup);
            RequestExcludeFromCurrentGroupCommand = new DelegateCommand(RequestExcludeFromCurrentGroup, CanRequestExcludeFromCurrentGroup)
                .ObservesProperty(() => IsIncludedInCurrentGroup);
        }

        public Permission Permission { get; private set; }

        public string Name { get { return Permission.Name; } }

        public string Description { get { return Permission.Description; } }

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

        public bool IsInGroupMode { get { return groupMode != null; } }

        public bool IsIncludedInCurrentGroup { get { return groupMode != null && Permission.PermissionGroupMemberships.Any(x => x.GroupId == groupMode.Group.Id); } }

        public ICommand RequestIncludeInCurrentGroupCommand { get; private set; }

        private void RequestIncludeInCurrentGroup()
        {
            OnIncludeInCurrentGroupRequested();
        }

        private bool CanRequestIncludeInCurrentGroup()
        {
            return IsInGroupMode && !IsIncludedInCurrentGroup;
        }

        public event EventHandler IncludeInCurrentGroupRequested;

        protected virtual void OnIncludeInCurrentGroupRequested()
        {
            var handler = IncludeInCurrentGroupRequested;
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
    }
}
