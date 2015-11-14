using System;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NonCachableAttribute : Attribute
    {
    }
}
