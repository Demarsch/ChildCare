using System;
using Core.Data;
using Prism.Mvvm;

namespace AdminModule.ViewModels
{
    public class PermissionViewModel : BindableBase
    {
        private readonly Permission permission;

        public PermissionViewModel(Permission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
            this.permission = permission;
        }

        public string Name { get { return permission.Name; } }

        public string Description { get { return permission.Description; } }
    }
}
