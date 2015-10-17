using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task<T[]> ToArrayAsync<T>(this IEnumerable<T> enumerable)
        {
            return await Task.Run(() => enumerable.ToArray());
        }
    }
}
