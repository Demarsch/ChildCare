using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Core.Services;

namespace Core.Extensions
{
    public static class CacheServiceExtensions
    {
        public static TSource AutoWire<TSource>(this ICacheService cacheService, TSource sourceItem, params Expression<Func<TSource, object>>[] propertyExpressions) where TSource : class
        {
            if (sourceItem == null)
            {
                return null;
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (propertyExpressions == null || propertyExpressions.Length == 0)
            {
                throw new ArgumentException("At least one property expression must be specified", "propertyExpressions");
            }
            var type = sourceItem.GetType();
            foreach (var property in propertyExpressions.Select(x => x.Body)
                .Cast<MemberExpression>()
                .Select(x => x.Member)
                .Cast<PropertyInfo>())
            {
                var idPropertySuggestedName = property.Name + "Id";
                var idProperty = type.GetProperty(idPropertySuggestedName);
                if (idProperty == null || idProperty.PropertyType != typeof(int))
                {
                    throw new InvalidOperationException(string.Format("Object of type {0} must have int property {1} for auto wire", type.Name, idPropertySuggestedName));
                }
                property.SetValue(sourceItem, cacheService.GetItemById<TSource>((int)idProperty.GetValue(sourceItem)));
            }
            return sourceItem;
        }
    }
}
