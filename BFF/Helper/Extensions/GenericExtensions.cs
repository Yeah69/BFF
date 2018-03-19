﻿using System.Collections;
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
    }
}
