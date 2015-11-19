using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BFF.WPFStuff.AttachedProperties;
using Brushes = System.Windows.Media.Brushes;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid
    {

        public TitDataGrid()
        {
            InitializeComponent();
        }

        public void RefreshCurrencyVisuals()
        {
            TitGrid.Items.Refresh();
            TotalBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        
        private void TitGrid_TextBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = ((TextBox) sender);
            textBox.IsReadOnly = false;
            textBox.BorderThickness = new Thickness(1.0);
        }

        private void TitGrid_TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            textBox.IsReadOnly = true;
            textBox.BorderThickness = new Thickness(0.0);
            textBox.Select(0,0);
        }

        private void CheckBox_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox?.SetValue(CTB.CollapseToggleButtonsProperty, false);
            comboBox?.SetValue(ComboBox.BackgroundProperty, Resources["ControlBackgroundBrush"]);
            comboBox?.SetValue(ComboBox.BorderThicknessProperty, new Thickness(1.0));
        }

        private void CheckBox_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox?.SetValue(CTB.CollapseToggleButtonsProperty, true);
            comboBox?.SetValue(ComboBox.BackgroundProperty, Brushes.Transparent);
            comboBox?.SetValue(ComboBox.BorderThicknessProperty, new Thickness(0.0));
        }
    }
}
