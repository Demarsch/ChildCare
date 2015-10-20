using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading;

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
                     if (sharedContext != null)
                {
                    return sharedContext;
                }
                    if (isDisposed)
                    {
                        throw new ObjectDisposedException("SharedContext");
                    }
                    var local = CreateNewContext();
                    local.Configuration.AutoDetectChangesEnabled = false;
                    local.Configuration.ProxyCreationEnabled = false;
                    Thread.MemoryBarrier();
                    sharedContext = local;
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
