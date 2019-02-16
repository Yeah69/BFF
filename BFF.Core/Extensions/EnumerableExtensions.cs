using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BFF.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task<IEnumerable<T>> ToAwaitableEnumerable<T>(this IEnumerable<Task<T>> enumerable) =>
            await Task.WhenAll(enumerable).ConfigureAwait(false);

        public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable) =>
            new ReadOnlyCollection<T>(enumerable.ToList());

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> enumerable) =>
            new ReadOnlyCollection<T>(enumerable.ToList());
    }
}
