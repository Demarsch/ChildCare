using System;
using System.Linq;

namespace Core.Misc
{
    public interface IDisposableQueryable<out T> : IQueryable<T>, IDisposable
    {
    }
}
