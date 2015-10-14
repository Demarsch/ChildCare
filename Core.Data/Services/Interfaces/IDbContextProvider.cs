using System;
using System.Data.Entity;

namespace Core.Data.Services
{
    public interface IDbContextProvider : IDisposable
    {
        DbContext SharedContext { get; }

        DbContext CreateNewContext();
    }
}
