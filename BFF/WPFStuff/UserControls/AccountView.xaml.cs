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

        private void RefreshDataGrids()
        {
            TitGrid.Items.Refresh();
            NewTitGrid.Items.Refresh();
        }

        public void RefreshCurrencyVisuals()
        {
            RefreshDataGrids();
            BalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            StartingBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        public void RefreshDateVisuals()
        {
            RefreshDataGrids();
        }

        private void AccountView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TitGrid.Items.Refresh();
        }

        private void CategoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshDataGrids();
        }
    }
}
