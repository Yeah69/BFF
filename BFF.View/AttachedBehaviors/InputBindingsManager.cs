using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace BFF.View.AttachedBehaviors
{
    public static class InputBindingsManager
    {

        public static readonly DependencyProperty UpdatePropertySourceWhenEnterPressedProperty =
            DependencyProperty.RegisterAttached(
                "UpdatePropertySourceWhenEnterPressed", typeof(DependencyProperty), typeof(InputBindingsManager),
                new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedPropertyChanged));

        static InputBindingsManager()
        {

        }

        public static void SetUpdatePropertySourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
        {
            dp.SetValue(UpdatePropertySourceWhenEnterPressedProperty, value);
        }

        public static DependencyProperty? GetUpdatePropertySourceWhenEnterPressed(DependencyObject? dp) => 
            dp?.GetValue(UpdatePropertySourceWhenEnterPressedProperty) as DependencyProperty;

        private static void OnUpdatePropertySourceWhenEnterPressedPropertyChanged(DependencyObject dp,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is UIElement element))
            {
                return;
            }

            if (e.OldValue is not null)
            {
                element.PreviewKeyDown -= HandlePreviewKeyDown;
            }

            if (e.NewValue is not null)
            {
                element.PreviewKeyDown += HandlePreviewKeyDown;
            }
        }

        static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoUpdateSource(e.Source);
            }
        }

        static void DoUpdateSource(object source)
        {
            DependencyProperty? property =
                GetUpdatePropertySourceWhenEnterPressed(source as DependencyObject);

            if (property is null)
            {
                return;
            }

            if (!(source is UIElement elt))
            {
                return;
            }

            BindingExpression binding = BindingOperations.GetBindingExpression(elt, property);

            binding?.UpdateSource();
        }
    }
}
