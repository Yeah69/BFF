using System.Windows;
using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class AccountView : IRefreshCurrencyVisuals, IRefreshDateVisuals
    {
        public AccountView()
        {
            InitializeComponent();
        }

        public void RefreshCurrencyVisuals()
        {
            TitGrid.Items.Refresh();
            NewTitGrid.Items.Refresh();
            TotalBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            StartingBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        public void RefreshDateVisuals()
        {
            TitGrid.Items.Refresh();
            NewTitGrid.Items.Refresh();
        }

        private void TitDataGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            TitGrid.Items.Refresh();
        }
    }
}
