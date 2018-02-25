using System.Windows;

namespace BFF.Helper.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T SetDependencyProperty<T>(this T dependencyObject, DependencyProperty property, object value) where T : DependencyObject
        {
            dependencyObject.SetValue(property, value);
            return dependencyObject;
        }
    }
}
