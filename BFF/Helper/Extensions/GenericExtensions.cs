using System;
using System.Collections.Generic;
using System.Linq;

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
            yield return element;
        }

        public static IEnumerable<T> IterateRootBreadthFirst<T>(this T root, Func<T, IEnumerable<T>> childrenSelector)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (childrenSelector == null) throw new ArgumentNullException(nameof(childrenSelector));

            return Inner();

            IEnumerable<T> Inner()
            {
                var queue = new Queue<T>();
                queue.Enqueue(root);

                while (queue.Any())
                {
                    var current = queue.Dequeue();
                    yield return current;
                    foreach (var child in childrenSelector(current))
                        queue.Enqueue(child);
                }
            }
        }

        public static IEnumerable<T> IterateRootDepthFirst<T>(this T root, Func<T, IEnumerable<T>> childrenSelector)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (childrenSelector == null) throw new ArgumentNullException(nameof(childrenSelector));

            return Inner();

            IEnumerable<T> Inner()
            {
                var stack = new Stack<T>();
                stack.Push(root);

                while (stack.Any())
                {
                    var current = stack.Pop();
                    yield return current;
                    foreach (var child in childrenSelector(current).Reverse())
                        stack.Push(child);
                }
            }
        }
    }
}
