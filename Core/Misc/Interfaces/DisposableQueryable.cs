using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Misc
{
    public sealed class DisposableQueryable<T> : IDisposableQueryable<T>
    {
        private static readonly DisposableQueryable<T> emptyInstance = new DisposableQueryable<T>(new T[0].AsQueryable(), null);

        public static DisposableQueryable<T> EmptyInstance { get { return emptyInstance; } }

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
