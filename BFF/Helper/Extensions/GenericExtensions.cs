using System;
using System.Collections.Generic;

namespace BFF.Helper.Extensions
{
    public static class GenericExtensions
    {
        public static bool IsNull<T>(this T element) where T : class => element is null;

        public static bool IsNull<T>(this T? element) where T : struct => element is null;

        public static bool IsNotNull<T>(this T element) where T : class => !(element is null);

        public static bool IsNotNull<T>(this T? element) where T : struct => !(element is null);

        public static T AddTo<T>(this T element, ICollection<T> collection)
        {
            collection.Add(element);
            return element;
        }

        public static T AddHere<T>(this T element, ICollection<IDisposable> collection) where T : IDisposable
        {
            collection.Add(element);
            return element;
        }

        public static IEnumerable<T> ToEnumerable<T>(this T element)
        {
            return new[] {element};
        }
    }
}
