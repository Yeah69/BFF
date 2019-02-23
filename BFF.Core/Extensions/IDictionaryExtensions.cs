using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BFF.Core.Extensions
{
    public static class IDictionaryExtensions
    {
        public static void AddToKey<TKey, TItem>(this IDictionary<TKey, IList<TItem>> @this, TKey key, TItem item)
        {
            if (!@this.TryGetValue(key, out var list) || list is null)
            {
                @this[key] = new List<TItem> { item };
            }
            list.Add(item);
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> @this) =>
            new ReadOnlyDictionary<TKey, TValue>(@this);
    }
}
