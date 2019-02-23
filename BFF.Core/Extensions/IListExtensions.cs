using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BFF.Core.Extensions
{
    public static class IListExtensions
    {
        public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IList<T> list) =>
            new ReadOnlyCollection<T>(list);

        public static IReadOnlyList<T> ToReadOnlyList<T>(this IList<T> list) =>
            new ReadOnlyCollection<T>(list);
    }
}
