using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MahApps.Metro.Controls;

namespace BFF.WPFStuff.AttachedProperties
{
    class CTB : ComboBox
    {
        public static DependencyProperty CollapseToggleButtonsProperty = DependencyProperty.RegisterAttached("CollapseToggleButton", typeof(bool), typeof(CTB), new PropertyMetadata(false, CollapseToggleButtonChanged));

        public static bool GetCollapseToggleButtons(DependencyObject d)
        {
            return (bool)d.GetValue(CollapseToggleButtonsProperty);
        }

        public static void SetCollapseToggleButtonsProperty(DependencyObject d, bool value)
        {
            d.SetValue(CollapseToggleButtonsProperty, value);
        }

        public static void CollapseToggleButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ComboBox comboBox = d as ComboBox;

            if (!comboBox.IsLoaded)
            {
                comboBox.Loaded += CTB_OnLoaded;
            }
            else
            {
                setVisibiliyOfToggleButton(comboBox, (bool)args.NewValue);
            }
        }

        private static void setVisibiliyOfToggleButton(ComboBox comboBox, bool collapsed)
        {
            ToggleButton toggleButton = comboBox.FindChildren<ToggleButton>(true).First();

            toggleButton.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
        }

        private static void CTB_OnLoaded(object sender, RoutedEventArgs args)
        {
            setVisibiliyOfToggleButton(sender as ComboBox, GetCollapseToggleButtons(sender as ComboBox));
            (sender as ComboBox).Loaded -= CTB_OnLoaded;
        }
    }
}
