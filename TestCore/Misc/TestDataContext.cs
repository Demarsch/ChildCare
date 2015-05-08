using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;

namespace TestCore
{
    public class TestDataContext : IDataContext
    {
        private readonly Dictionary<Type, IEnumerable> data = new Dictionary<Type, IEnumerable>(); 

        public void Dispose()
        {
        }

        public void AddData<TData>(ICollection<TData> collection) where TData : class
        {
            data[typeof(TData)] = collection;
        }

        public IQueryable<TData> GetData<TData>() where TData : class
        {
            IEnumerable result;
            return data.TryGetValue(typeof(TData), out result) ? (result as ICollection<TData>).AsQueryable() : new TData[0].AsQueryable();
        }

        public TData GetById<TData>(int id) where TData : class
        {
            throw new NotImplementedException();
        }

        public ICollection<TData> GetAll<TData>() where TData : class
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Add<TData>(TData obj) where TData : class
        {
            throw new NotImplementedException();
        }

        public void AddRange<TData>(IEnumerable<TData> obj) where TData : class
        {
            throw new NotImplementedException();
        }

        public void Remove<TData>(TData obj) where TData : class
        {
            throw new NotImplementedException();
        }

        public void RemoveRange<TData>(IEnumerable<TData> obj) where TData : class
        {
            throw new NotImplementedException();
        }
    }
}
