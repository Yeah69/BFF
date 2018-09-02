using System.Windows;
using System.Windows.Media;

namespace BFF.Helper.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T SetDependencyProperty<T>(this T dependencyObject, DependencyProperty property, object value) where T : DependencyObject
        {
            dependencyObject.SetValue(property, value);
            return dependencyObject;
        }

        public static T FindVisualChild<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T t)
                    return t;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
