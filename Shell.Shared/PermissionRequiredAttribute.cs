using System;

namespace Shell.Shared
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PermissionRequiredAttribute : Attribute
    {
        public PermissionRequiredAttribute(string permissionName)
        {
            PermissionName = permissionName;
        }

        public string PermissionName { get; private set; }
    }
}
