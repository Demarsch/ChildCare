using System;
using System.Linq;

namespace Core
{
    public interface IDataContext : IDisposable
    {
        IQueryable<TData> GetData<TData>() where TData : class;

        void SetState<TData>(TData obj, DataContextItemState state) where TData : class;

        void Save();

        void Add<TData>(TData obj) where TData : class;
    }

    public enum DataContextItemState { Add, Update, Delete, None }
}
