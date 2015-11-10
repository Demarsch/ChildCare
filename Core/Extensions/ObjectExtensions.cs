using System;

namespace Core.Extensions
{
    public static class ObjectExtensions
    {
        public static void SetValue(this object source, string propertyName, object value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name can't be empty", "propertyName");
            }
            source.GetType().GetProperty(propertyName).SetValue(source, value);
        }

        public static object GetValue(this object source, string propertyName)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name can't be empty", "propertyName");
            }
            return source.GetType().GetProperty(propertyName).GetValue(source);
        }
    }
}
