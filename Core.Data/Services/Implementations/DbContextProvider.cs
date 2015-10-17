using System;
using System.Data.Entity;
using System.Diagnostics;

namespace Core.Data.Services
{
    public class DbContextProvider : IDbContextProvider
    {
        private readonly object sharedContextLock = new object();

        private bool isDisposed;

        private DbContext sharedContext;

        public void Dispose()
        {
            lock (sharedContextLock)
            {
                if (sharedContext == null)
                {
                    return;
                }
                sharedContext.Dispose();
                sharedContext = null;
                isDisposed = true;
            }
        }

        public DbContext SharedContext
        {
            get
            {
                if (sharedContext != null)
                {
                    return sharedContext;
                }
                lock (sharedContextLock)
                {
                    if (isDisposed)
                    {
                        throw new ObjectDisposedException("SharedContext");
                    }
                    sharedContext = CreateNewContext();
                    sharedContext.Configuration.AutoDetectChangesEnabled = false;
                    sharedContext.Configuration.ProxyCreationEnabled = false;
                }
                return sharedContext;
            }
        }

        public DbContext CreateNewContext()
        {
            var result = new ModelContext();
#if DEBUG
            result.Database.Log = x => Debug.Write(x);
#endif
            return result;
        }
    }
}
