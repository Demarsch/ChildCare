using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;

namespace Core.Implementations
{
    public sealed class DisposableQueryable<T> : IDisposableQueryable<T>
    {
        private readonly IQueryable<T> query;

        private IDisposable disposable;

        public DisposableQueryable(IQueryable<T> query, IDisposable disposable)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            this.query = query;
            this.disposable = disposable;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get { return query.Expression; } }

        public Type ElementType { get { return query.ElementType; } }

        public IQueryProvider Provider { get { return query.Provider; } }

        public void Dispose()
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }
    }
}
