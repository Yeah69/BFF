using System.Windows;
using System.Windows.Media;

namespace BFF.View.Extensions
{
    public static class VisualExtensions
    {
        public static T? GetDescendantByType<T>(this Visual? element) where T : class
        {
            if (element is null)
            {
                return default;
            }
            if (element.GetType() == typeof(T))
            {
                return element as T;
            }
            T? foundElement = null;
            (element as FrameworkElement)?.ApplyTemplate();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = visual.GetDescendantByType<T>();
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
        }

    }
}
