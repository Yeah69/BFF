using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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

        public void refreshCurrencyVisuals()
        {
            TitGrid.Items.Refresh();
            TotalBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        }

        
        private void TitGrid_SumBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = ((TextBox) sender);
            textBox.IsReadOnly = false;
            textBox.BorderThickness = new Thickness(1.0);
        }

        private void TitGrid_SumBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            textBox.IsReadOnly = true;
            textBox.BorderThickness = new Thickness(0.0);
            textBox.Select(0,0);
        }
    }
}
