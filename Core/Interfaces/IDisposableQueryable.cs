using System;
using System.Linq;

namespace Core.Interfaces
{
    public interface IDisposableQueryable<T> : IQueryable<T>, IDisposable
    {
    }
}
