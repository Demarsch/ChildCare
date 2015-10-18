using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Data.Misc
{
    internal class EmptyDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        public static readonly EmptyDbAsyncEnumerator<T> Empty = new EmptyDbAsyncEnumerator<T>();

        public void Dispose()
        {
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            var result = new TaskCompletionSource<bool>();
            result.SetResult(false);
            return result.Task;
        }

        public T Current { get { return default(T); } }

        object IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }
    }
}