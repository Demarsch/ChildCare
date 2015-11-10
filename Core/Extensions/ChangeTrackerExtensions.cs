using System;
using System.Collections;
using System.Linq.Expressions;
using Core.Misc;

namespace Core.Extensions
{
    public static class ChangeTrackerExtensions
    {
        public static void RegisterComparer<TValue>(this IChangeTracker changeTracker, Expression<Func<TValue>> propertyExpression, IEqualityComparer comparer)
        {
            if (changeTracker == null)
            {
                throw new ArgumentNullException("changeTracker");
            }
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            changeTracker.RegisterComparer(propertyName, comparer);
        }

        public static bool PropertyHasChanges<TValue>(this IChangeTracker changeTracker, Expression<Func<TValue>> propertyExpression)
        {
            if (changeTracker == null)
            {
                throw new ArgumentNullException("changeTracker");
            }
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            return changeTracker.PropertyHasChanges(propertyName);
        }
    }
}
