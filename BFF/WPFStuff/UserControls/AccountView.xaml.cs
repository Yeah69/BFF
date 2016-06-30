using System;
using System.Windows;
using System.Windows.Controls;
using BFF.DB;
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
            //TitGrid.Items.Refresh(); //todo !!!
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
            RefreshDataGrids();
        }
    }
}
