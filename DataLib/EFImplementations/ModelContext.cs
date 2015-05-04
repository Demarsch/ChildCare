using System;
using System.Linq;
using Core;
using System.Data.Entity;

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

        public void SetState<TData>(TData obj, DataContextItemState state) where TData : class
        {
            Entry(obj).State = (state == DataContextItemState.Add) ? EntityState.Added : (state == DataContextItemState.Delete ? EntityState.Deleted : (state == DataContextItemState.Update ? EntityState.Modified : EntityState.Unchanged));
        }

        public void Save()
        {
            SaveChanges();
        }

        public void Add<TData>(TData obj) where TData : class
        {
            Set<TData>().Add(obj);
        }

        public DateTime GetCurrentDate()
        {
            return Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstAsync().Result;
        }
    }
}
