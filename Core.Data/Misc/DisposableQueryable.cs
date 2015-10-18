using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Data.Misc
{
    public sealed class DisposableQueryable<T> : IDisposableQueryable<T>
    {
        public static readonly DisposableQueryable<T> Empty = new DisposableQueryable<T>(EmptyDbAsyncEnumerable<T>.Empty, null);

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

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            var result = query as IDbAsyncEnumerable<T>;
            if (result == null)
            {
                throw new InvalidOperationException(string.Format("Type {0} doesn't implement IDbAsyncEnumerable<{1}> interface", query.GetType(), typeof(T).Name));
            }
            return result.GetAsyncEnumerator();
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }
    }
}
