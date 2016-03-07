using System.Windows;
using System.Windows.Controls;
using BFF.Helper;
using BFF.Model.Native;

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
            //TitGrid.Items.Refresh();
            //NewTitGrid.Items.Refresh();
        }

        public void RefreshCurrencyVisuals()
        {
            RefreshDataGrids();
            BalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            StartingBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        public void RefreshDateVisuals()
        {
            //RefreshDataGrids();
        }

        private void TitGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            //((DataGrid) sender).Items.Refresh();
        }

        private void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        {
            //TitGrid.Items.Refresh();
        }

        private void AccountView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Account account = DataContext as Account;
            if(account != null)
                account.RefreshDataGrid = () => { TitGrid.Items.Refresh(); };
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            TitGrid.UnselectAllCells();
        }
    }
}
