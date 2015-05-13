using System;
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

        public void Add<TData>(TData obj) where TData : class
        {
            Set<TData>().Add(obj);
        }

        public DateTime GetCurrentDate()
        {
            return Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstAsync().Result;
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

        public void Update<TData>(TData obj) where TData : class
        {
            this.Entry<TData>(obj).State = EntityState.Modified;
        }

        public void Save()
        {
            SaveChanges();
        }
    }
}
