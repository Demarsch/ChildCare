using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Data.Misc
{
    internal class EmptyDbAsyncEnumerable<T> : IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public static readonly EmptyDbAsyncEnumerable<T> Empty = new EmptyDbAsyncEnumerable<T>();

        private static readonly IQueryable<T> QueryableEmpty = new T[0].AsQueryable(); 

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return EmptyDbAsyncEnumerator<T>.Empty;
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return QueryableEmpty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get { return QueryableEmpty.Expression; } }

        public Type ElementType { get { return QueryableEmpty.ElementType; } }

        public IQueryProvider Provider { get { return EmptyDbAsyncQueryProvider.Empty; } }
    }
}
