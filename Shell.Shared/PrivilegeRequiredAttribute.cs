using System;

namespace Shell.Shared
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PrivilegeRequiredAttribute : Attribute
    {
        public PrivilegeRequiredAttribute(string privilegeName)
        {
            PrivilegeRequired = privilegeName;
        }

        public string PrivilegeRequired { get; private set; }
    }
}
