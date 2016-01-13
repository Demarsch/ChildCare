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

        public static void ResetChanges<TItem>(this DbContext context) where TItem : class
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            foreach (var entry in context.ChangeTracker.Entries<TItem>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
