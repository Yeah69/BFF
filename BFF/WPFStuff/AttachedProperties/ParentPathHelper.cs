using System.Windows;
using System.Windows.Controls;

namespace BFF.WPFStuff.AttachedProperties
{
    class ParentPathHelper : ComboBox
    {
        public static DependencyProperty ParentPathProperty = DependencyProperty.RegisterAttached("ParentPath", typeof(string), typeof(ParentPathHelper), new PropertyMetadata("Parent", ParentPathChanged));

        public static string GetParentPath(DependencyObject d)
        {
            return (string)d.GetValue(ParentPathProperty);
        }

        public static void SetParentPath(DependencyObject d, string value)
        {
            d.SetValue(ParentPathProperty, value);
        }

        public static void ParentPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {

        }
    }
}
