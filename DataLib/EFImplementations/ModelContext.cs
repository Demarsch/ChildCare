using System.Linq;
using Core;
using System.Data.Entity;
using System.Collections.Generic;

namespace DataLib
{
    public partial class ModelContext : IDataContext
    {
        public ModelContext(string connectionString)
            : base(connectionString)
        {
        }

        public IQueryable<TData> GetData<TData>() where TData : class
        {
            return Set<TData>();
        }

        public TData GetById<TData>(int id) where TData : class
        {
            return Set<TData>().ById(id);
        }

        public ICollection<TData> GetAll<TData>() where TData : class
        {
            return Set<TData>().ToArray();
        }

        public void Add<TData>(TData obj) where TData : class
        {
            this.Set<TData>().Add(obj);
        }

        public void AddRange<TData>(IEnumerable<TData> objs) where TData : class
        {
            this.Set<TData>().AddRange(objs);
        }

        public void Remove<TData>(TData obj) where TData : class
        {
            this.Set<TData>().Remove(obj);
        }

        public void RemoveRange<TData>(IEnumerable<TData> objs) where TData : class
        {
            this.Set<TData>().RemoveRange(objs);
        }

        public void Save()
        {
            SaveChanges();
        }
    }
}
