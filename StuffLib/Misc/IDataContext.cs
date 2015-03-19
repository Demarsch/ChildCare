using System;
using System.Linq;

namespace Core
{
    public interface IDataContext : IDisposable
    {
        IQueryable<TData> GetData<TData>() where TData : class;
    }
}
