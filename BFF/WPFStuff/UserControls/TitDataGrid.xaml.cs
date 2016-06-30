using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using BFF.Model.Native;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid
    {
        #region Depencency Properties

        public static readonly DependencyProperty AccountProperty = DependencyProperty.Register(
            nameof(Account), typeof(Account), typeof(TitDataGrid),
            new PropertyMetadata(default(Account), OnAccountChanged));

        public Account Account
        {
            get { return (Account) GetValue(AccountProperty); }
            set { SetValue(AccountProperty, value); }
        }

        public static readonly DependencyProperty TitsProperty = DependencyProperty.Register(
            nameof(Tits), typeof(IEnumerable), typeof(TitDataGrid),
            new PropertyMetadata(default(IEnumerable)));

        public IEnumerable Tits
        {
            get { return (IEnumerable) GetValue(TitsProperty); }
            set { SetValue(TitsProperty, value); }
        }

        public static readonly DependencyProperty NewTitsProperty = DependencyProperty.Register(
            nameof(NewTits), typeof(IEnumerable), typeof(TitDataGrid), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable NewTits
        {
            get { return (IEnumerable) GetValue(NewTitsProperty); }
            set { SetValue(NewTitsProperty, value); }
        }

        public static readonly DependencyProperty NewTransactionCommandProperty = DependencyProperty.Register(
            nameof(NewTransactionCommand), typeof(ICommand), typeof(TitDataGrid),
            new PropertyMetadata(default(ICommand)));

        public ICommand NewTransactionCommand
        {
            get { return (ICommand) GetValue(NewTransactionCommandProperty); }
            set { SetValue(NewTransactionCommandProperty, value); }
        }

        public static readonly DependencyProperty NewIncomeCommandProperty = DependencyProperty.Register(
            nameof(NewIncomeCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand)));

        public ICommand NewIncomeCommand
        {
            get { return (ICommand) GetValue(NewIncomeCommandProperty); }
            set { SetValue(NewIncomeCommandProperty, value); }
        }

        public static readonly DependencyProperty NewTransferCommandProperty = DependencyProperty.Register(
            nameof(NewTransferCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand)));

        public ICommand NewTransferCommand
        {
            get { return (ICommand) GetValue(NewTransferCommandProperty); }
            set { SetValue(NewTransferCommandProperty, value); }
        }

        public static readonly DependencyProperty NewParentTransactionCommandProperty = DependencyProperty.Register(
            nameof(NewParentTransactionCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand)));

        public ICommand NewParentTransactionCommand
        {
            get { return (ICommand) GetValue(NewParentTransactionCommandProperty); }
            set { SetValue(NewParentTransactionCommandProperty, value); }
        }

        public static readonly DependencyProperty NewParentIncomeCommandProperty = DependencyProperty.Register(
            nameof(NewParentIncomeCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand)));

        public ICommand NewParentIncomeCommand
        {
            get { return (ICommand) GetValue(NewParentIncomeCommandProperty); }
            set { SetValue(NewParentIncomeCommandProperty, value); }
        }

        public static readonly DependencyProperty ApplyCommandProperty = DependencyProperty.Register(
            nameof(ApplyCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand)));

        public ICommand ApplyCommand
        {
            get { return (ICommand) GetValue(ApplyCommandProperty); }
            set { SetValue(ApplyCommandProperty, value); }
        }

        #endregion


        public TitDataGrid()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;
        }

        private static void OnAccountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            Account oldAccount = (Account) args.OldValue;
            if (oldAccount != null)
            {
                oldAccount.PreVirtualizedRefresh -= ((TitDataGrid)sender).PreVirtualizedRefresh;
                oldAccount.PostVirtualizedRefresh -= ((TitDataGrid)sender).PostVirtualizedRefresh;
            }
            Account newAccount = (Account) args.NewValue;
            if (newAccount != null)
            {
                newAccount.PreVirtualizedRefresh += ((TitDataGrid)sender).PreVirtualizedRefresh;
                newAccount.PostVirtualizedRefresh += ((TitDataGrid)sender).PostVirtualizedRefresh;
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
