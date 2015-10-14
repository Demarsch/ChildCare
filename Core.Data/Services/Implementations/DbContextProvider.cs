using System;
using System.Data.Entity;

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
                    sharedContext = new ModelContext();
                    sharedContext.Configuration.AutoDetectChangesEnabled = false;
                }
                return sharedContext;
            }
        }

        public DbContext CreateNewContext()
        {
            return new ModelContext();
        }
    }
}
