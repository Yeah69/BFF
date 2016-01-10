using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BFF.Helper;
using BFF.ViewModel;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid : IRefreshCurrencyVisuals
    {
        public TitDataGrid()
        {
            InitializeComponent();
        }

        public TitDataGrid(TitViewModel titViewModel) : this()
        {
            DataContext = titViewModel;
        }

        public void RefreshCurrencyVisuals()
        {
            TitGrid.Items.Refresh();
            TotalBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        
        private void TitGrid_TextBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = (TextBox) sender;
            textBox.IsReadOnly = false;
            textBox.BorderThickness = new Thickness(1.0);
        }

        private void TitGrid_TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.IsReadOnly = true;
            textBox.BorderThickness = new Thickness(0.0);
            textBox.Select(0,0);
        }
    }
}
