using System;
using System.Linq.Expressions;
using System.Reflection;
using Core.Services;

namespace Core.Extensions
{
    public static class CacheServiceExtensions
    {
        public static TSource AutoWire<TSource, TProperty>(this ICacheService cacheService, TSource sourceItem, Expression<Func<TSource, TProperty>> propertyExpression)
            where TSource : class
            where TProperty : class
        {
            if (sourceItem == null)
            {
                return null;
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            //TODO: is it worth to put additional checks here?
            var type = sourceItem.GetType();
            var property = (propertyExpression.Body as MemberExpression).Member as PropertyInfo;
            var idPropertySuggestedName = property.Name + "Id";
            var idProperty = type.GetProperty(idPropertySuggestedName);
            if (idProperty == null || idProperty.PropertyType != typeof(int))
            {
                throw new InvalidOperationException(string.Format("Object of type {0} must have int property {1} for auto wire", type.Name, idPropertySuggestedName));
            }
            property.SetValue(sourceItem, cacheService.GetItemById<TProperty>((int)idProperty.GetValue(sourceItem)));
            return sourceItem;
        }
    }
}