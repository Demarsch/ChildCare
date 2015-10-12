using System;
using System.Data.Entity;
using System.Linq;
using Core;
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
            Set<TData>().AddRange(objs);
        }

        public void Remove<TData>(TData obj) where TData : class
        {
            if (Entry(obj).State == EntityState.Detached)
            {
                Entry(obj).State = EntityState.Deleted;
            }
            else
            {
                Set<TData>().Remove(obj);
            }
        }

        public void RemoveRange<TData>(IEnumerable<TData> objs) where TData : class
        {
            Set<TData>().RemoveRange(objs);
        }

        public void Update<TData>(TData obj) where TData : class
        {
            Entry(obj).State = EntityState.Modified;
        }

        public void Save()
        {
            SaveChanges();
        }
    }
}
