using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task<T[]> ToArrayAsync<T>(this IEnumerable<T> enumerable)
        {
            return await ToArrayAsync(enumerable, CancellationToken.None);
        }

        public static async Task<T[]> ToArrayAsync<T>(this IEnumerable<T> enumerable, CancellationToken token)
        {
            return await Task.Run(() => enumerable.ToArray(), token);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static IEnumerable<T> WithAction<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
                yield return item;
            }
        }
    }
}
