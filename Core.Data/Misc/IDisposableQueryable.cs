using System;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Core.Data.Misc
{
    public interface IDisposableQueryable<out T> : IQueryable<T>, IDisposable, IDbAsyncEnumerable<T>
    {
    }
}
