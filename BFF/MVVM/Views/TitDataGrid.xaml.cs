using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.Views
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid
    {
        #region Depencency Properties

        public static readonly DependencyProperty AccountProperty = DependencyProperty.Register(
            nameof(Account), typeof(IAccount), typeof(TitDataGrid), new PropertyMetadata(default(IAccount)));

        public IAccount Account
        {
            get { return (IAccount) GetValue(AccountProperty); }
            set { SetValue(AccountProperty, value); }
        }

        public static readonly DependencyProperty AccountViewModelProperty = DependencyProperty.Register(
            nameof(AccountViewModel), typeof(AccountBaseViewModel), typeof(TitDataGrid), 
            new PropertyMetadata(default(AccountBaseViewModel), OnAccountViewModelChanged));

        public AccountBaseViewModel AccountViewModel
        {
            get { return (AccountBaseViewModel) GetValue(AccountViewModelProperty); }
            set { SetValue(AccountViewModelProperty, value); }
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
            nameof(NewTransactionCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).NewTransactionVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewTransactionCommand
        {
            get { return (ICommand) GetValue(NewTransactionCommandProperty); }
            set { SetValue(NewTransactionCommandProperty, value); }
        }

        public static readonly DependencyProperty NewIncomeCommandProperty = DependencyProperty.Register(
            nameof(NewIncomeCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand),(o, args) =>
            {
                ((TitDataGrid)o).NewIncomeVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewIncomeCommand
        {
            get { return (ICommand) GetValue(NewIncomeCommandProperty); }
            set { SetValue(NewIncomeCommandProperty, value); }
        }

        public static readonly DependencyProperty NewTransferCommandProperty = DependencyProperty.Register(
            nameof(NewTransferCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).NewTransferVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewTransferCommand
        {
            get { return (ICommand) GetValue(NewTransferCommandProperty); }
            set { SetValue(NewTransferCommandProperty, value); }
        }

        public static readonly DependencyProperty NewParentTransactionCommandProperty = DependencyProperty.Register(
            nameof(NewParentTransactionCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).NewParentTransactionVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewParentTransactionCommand
        {
            get { return (ICommand) GetValue(NewParentTransactionCommandProperty); }
            set { SetValue(NewParentTransactionCommandProperty, value); }
        }

        public static readonly DependencyProperty NewParentIncomeCommandProperty = DependencyProperty.Register(
            nameof(NewParentIncomeCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).NewParentIncomeVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewParentIncomeCommand
        {
            get { return (ICommand) GetValue(NewParentIncomeCommandProperty); }
            set { SetValue(NewParentIncomeCommandProperty, value); }
        }

        public static readonly DependencyProperty ApplyCommandProperty = DependencyProperty.Register(
            nameof(ApplyCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).ApplyVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand ApplyCommand
        {
            get { return (ICommand) GetValue(ApplyCommandProperty); }
            set { SetValue(ApplyCommandProperty, value); }
        }

        private static readonly DependencyProperty NewTransactionVisibilityProperty = DependencyProperty.Register(
            nameof(NewTransactionVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        private Visibility NewTransactionVisibility
        {
            get { return (Visibility) GetValue(NewTransactionVisibilityProperty); }
            set { SetValue(NewTransactionVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NewIncomeVisibilityProperty = DependencyProperty.Register(
            nameof(NewIncomeVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NewIncomeVisibility
        {
            get { return (Visibility) GetValue(NewIncomeVisibilityProperty); }
            set { SetValue(NewIncomeVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NewTransferVisibilityProperty = DependencyProperty.Register(
            nameof(NewTransferVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NewTransferVisibility
        {
            get { return (Visibility) GetValue(NewTransferVisibilityProperty); }
            set { SetValue(NewTransferVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NewParentTransactionVisibilityProperty = DependencyProperty.Register(
            nameof(NewParentTransactionVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NewParentTransactionVisibility
        {
            get { return (Visibility) GetValue(NewParentTransactionVisibilityProperty); }
            set { SetValue(NewParentTransactionVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NewParentIncomeVisibilityProperty = DependencyProperty.Register(
            nameof(NewParentIncomeVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NewParentIncomeVisibility
        {
            get { return (Visibility) GetValue(NewParentIncomeVisibilityProperty); }
            set { SetValue(NewParentIncomeVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ApplyVisibilityProperty = DependencyProperty.Register(
            nameof(ApplyVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ApplyVisibility
        {
            get { return (Visibility) GetValue(ApplyVisibilityProperty); }
            set { SetValue(ApplyVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ShowRowDetailsModeProperty = DependencyProperty.Register(
            nameof(ShowRowDetailsMode), typeof(DataGridRowDetailsVisibilityMode), typeof(TitDataGrid), new PropertyMetadata(DataGridRowDetailsVisibilityMode.Collapsed));

        public DataGridRowDetailsVisibilityMode ShowRowDetailsMode
        {
            get { return (DataGridRowDetailsVisibilityMode) GetValue(ShowRowDetailsModeProperty); }
            set { SetValue(ShowRowDetailsModeProperty, value); }
        }

        public static readonly DependencyProperty IsDateLongProperty = DependencyProperty.Register(
            nameof(IsDateLong), typeof(bool), typeof(TitDataGrid), new PropertyMetadata(default(bool)));

        public bool IsDateLong
        {
            get { return (bool) GetValue(IsDateLongProperty); }
            set { SetValue(IsDateLongProperty, value); }
        }

        #endregion


        public TitDataGrid()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;
        }

        private static void OnAccountViewModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            AccountBaseViewModel oldAccount = (AccountBaseViewModel) args.OldValue;
            if (oldAccount != null)
            {
                oldAccount.PreVirtualizedRefresh -= ((TitDataGrid)sender).PreVirtualizedRefresh;
                oldAccount.PostVirtualizedRefresh -= ((TitDataGrid)sender).PostVirtualizedRefresh;
            }
            AccountBaseViewModel newAccount = (AccountBaseViewModel) args.NewValue;
            if (newAccount != null)
            {
                newAccount.PreVirtualizedRefresh += ((TitDataGrid)sender).PreVirtualizedRefresh;
                newAccount.PostVirtualizedRefresh += ((TitDataGrid)sender).PostVirtualizedRefresh;
            }
        }

        private int _previousPosition;

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
