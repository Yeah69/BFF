using System.Collections.Generic;
using System.Threading.Tasks;

namespace BFF.Helper.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task<IEnumerable<T>> ToAwaitableEnumerable<T>(this IEnumerable<Task<T>> enumerable) =>
            await Task.WhenAll(enumerable);
    }
}
