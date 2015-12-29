using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Core.Data.Misc
{
    public static class DbContextExtensions
    {
        public static DbQuery<TItem> NoTrackingSet<TItem>(this DbContext context) where TItem : class
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return context.Set<TItem>().AsNoTracking();
        }
    }
}
