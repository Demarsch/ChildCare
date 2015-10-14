using System;
using System.Linq;

namespace Core.Misc
{
    public interface IDisposableQueryable<T> : IQueryable<T>, IDisposable
    {
    }
}
