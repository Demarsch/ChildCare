using System;
using Core.Data;
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
            Name = group.Name;
            Description = group.Description;
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
    }
}
