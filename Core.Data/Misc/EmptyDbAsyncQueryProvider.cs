using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Data.Misc
{
    internal class EmptyDbAsyncQueryProvider : IDbAsyncQueryProvider
    {
        public static readonly EmptyDbAsyncQueryProvider Empty = new EmptyDbAsyncQueryProvider();

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return EmptyDbAsyncEnumerable<TElement>.Empty;
        }

        public object Execute(Expression expression)
        {
            return null;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return default(TResult);
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            var result = new TaskCompletionSource<object>();
            result.SetResult(null);
            return result.Task;
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var result = new TaskCompletionSource<TResult>();
            result.SetResult(default(TResult));
            return result.Task;
        }
    }
}
