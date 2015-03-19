using System.Linq;
using Core;

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
    }
}
