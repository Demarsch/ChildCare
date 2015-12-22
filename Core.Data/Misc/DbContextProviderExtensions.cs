using System;
using System.Data.Entity;
using Core.Data.Services;

namespace Core.Data.Misc
{
    public static class DbContextProviderExtensions
    {
        public static DbContext CreateLightweightContext(this IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            var result = contextProvider.CreateNewContext();
            result.Configuration.AutoDetectChangesEnabled = false;
            result.Configuration.LazyLoadingEnabled = false;
            result.Configuration.ProxyCreationEnabled = false;
            return result;
        }
    }
}
