using System;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IdPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public IdPropertyAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Property name must not be empty");
            }
            Name = name;
        }
    }
}