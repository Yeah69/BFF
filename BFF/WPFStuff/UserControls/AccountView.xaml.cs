using System.Windows;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class AccountView : IRefreshCurrencyVisuals
    {
        public AccountView()
        {
            InitializeComponent();
        }

        public void RefreshCurrencyVisuals()
        {
            TitGrid.Items.Refresh();
            TotalBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        private void TitDataGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            TitGrid.Items.Refresh();
        }
    }
}
