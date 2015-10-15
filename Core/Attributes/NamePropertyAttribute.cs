using System;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NamePropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public NamePropertyAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Property name must not be empty");
            }
            Name = name;
        }
    }
}