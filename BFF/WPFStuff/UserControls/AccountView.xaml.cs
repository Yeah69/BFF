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

        private void AccountView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IVirtualizedRefresh oldVirtualizedRefresh = e.OldValue as IVirtualizedRefresh;
            if(oldVirtualizedRefresh != null)
            {
                oldVirtualizedRefresh.PreVirtualizedRefresh -= PreVirtualizedRefresh;
                oldVirtualizedRefresh.PostVirtualizedRefresh -= PostVirtualizedRefresh;
            }
            IVirtualizedRefresh virtualizedRefresh = DataContext as IVirtualizedRefresh;
            if(virtualizedRefresh != null)
            {
                virtualizedRefresh.PreVirtualizedRefresh += PreVirtualizedRefresh;
                virtualizedRefresh.PostVirtualizedRefresh += PostVirtualizedRefresh;
            }
        }

        private int _previousPosition = 0;

        private void PreVirtualizedRefresh(object sender, EventArgs args)
        {
            _previousPosition = TitGrid.SelectedIndex;
            TitGrid.UnselectAllCells();
        }

        private void PostVirtualizedRefresh(object sender, EventArgs args)
        {
            if(TitGrid.Items.Count > _previousPosition)
            {
                if(_previousPosition != -1)
                {
                    TitGrid.CurrentItem = TitGrid.Items[_previousPosition];
                    TitGrid.ScrollIntoView(TitGrid.CurrentItem);
                }
                TitGrid.SelectedIndex = _previousPosition;
            }
        }
    }
}
