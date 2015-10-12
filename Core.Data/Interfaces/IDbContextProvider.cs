using System;
using System.Data.Entity;

namespace Core.Data.Interfaces
{
    public interface IDbContextProvider : IDisposable
    {
        DbContext SharedContext { get; }

        DbContext CreateNewContext();
    }
}
