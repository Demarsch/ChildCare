using System;
using System.Linq;

namespace Core
{
    public interface IDataContext : IDisposable
    {
        IQueryable<TData> GetData<TData>() where TData : class;

        void Save();

        void Add<TData>(TData obj) where TData : class;
    }
}
