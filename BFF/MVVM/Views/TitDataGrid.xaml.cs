﻿using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.Views
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid
    {
        #region Depencency Properties

        public static readonly DependencyProperty AccountViewModelProperty = DependencyProperty.Register(
            nameof(AccountViewModel), typeof(IAccountBaseViewModel), typeof(TitDataGrid), 
            new PropertyMetadata(default(IAccountBaseViewModel), OnAccountViewModelChanged));

        public IAccountBaseViewModel AccountViewModel
        {
            get => (IAccountBaseViewModel) GetValue(AccountViewModelProperty);
            set => SetValue(AccountViewModelProperty, value);
        }

        public static readonly DependencyProperty TitsProperty = DependencyProperty.Register(
            nameof(Tits), typeof(IEnumerable), typeof(TitDataGrid),
            new PropertyMetadata(default(IEnumerable)));

        public IEnumerable Tits
        {
            get => (IEnumerable) GetValue(TitsProperty);
            set => SetValue(TitsProperty, value);
        }

        public static readonly DependencyProperty NewTitsProperty = DependencyProperty.Register(
            nameof(NewTits), typeof(IEnumerable), typeof(TitDataGrid), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable NewTits
        {
            get => (IEnumerable) GetValue(NewTitsProperty);
            set => SetValue(NewTitsProperty, value);
        }

        public static readonly DependencyProperty NewTransactionCommandProperty = DependencyProperty.Register(
            nameof(NewTransactionCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).NewTransactionVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewTransactionCommand
        {
            get => (ICommand) GetValue(NewTransactionCommandProperty);
            set => SetValue(NewTransactionCommandProperty, value);
        }

        public static readonly DependencyProperty NewTransferCommandProperty = DependencyProperty.Register(
            nameof(NewTransferCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).NewTransferVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewTransferCommand
        {
            get => (ICommand) GetValue(NewTransferCommandProperty);
            set => SetValue(NewTransferCommandProperty, value);
        }

        public static readonly DependencyProperty NewParentTransactionCommandProperty = DependencyProperty.Register(
            nameof(NewParentTransactionCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).NewParentTransactionVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand NewParentTransactionCommand
        {
            get => (ICommand) GetValue(NewParentTransactionCommandProperty);
            set => SetValue(NewParentTransactionCommandProperty, value);
        }

        public static readonly DependencyProperty ApplyCommandProperty = DependencyProperty.Register(
            nameof(ApplyCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand), (o, args) =>
            {
                ((TitDataGrid)o).ApplyVisibility = args.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }));

        public ICommand ApplyCommand
        {
            get => (ICommand) GetValue(ApplyCommandProperty);
            set => SetValue(ApplyCommandProperty, value);
        }

        private static readonly DependencyProperty NewTransactionVisibilityProperty = DependencyProperty.Register(
            nameof(NewTransactionVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        private Visibility NewTransactionVisibility
        {
            get => (Visibility) GetValue(NewTransactionVisibilityProperty);
            set => SetValue(NewTransactionVisibilityProperty, value);
        }

        public static readonly DependencyProperty NewTransferVisibilityProperty = DependencyProperty.Register(
            nameof(NewTransferVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NewTransferVisibility
        {
            get => (Visibility) GetValue(NewTransferVisibilityProperty);
            set => SetValue(NewTransferVisibilityProperty, value);
        }

        public static readonly DependencyProperty NewParentTransactionVisibilityProperty = DependencyProperty.Register(
            nameof(NewParentTransactionVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NewParentTransactionVisibility
        {
            get => (Visibility) GetValue(NewParentTransactionVisibilityProperty);
            set => SetValue(NewParentTransactionVisibilityProperty, value);
        }

        public static readonly DependencyProperty ApplyVisibilityProperty = DependencyProperty.Register(
            nameof(ApplyVisibility), typeof(Visibility), typeof(TitDataGrid), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ApplyVisibility
        {
            get => (Visibility) GetValue(ApplyVisibilityProperty);
            set => SetValue(ApplyVisibilityProperty, value);
        }

        public static readonly DependencyProperty ShowRowDetailsModeProperty = DependencyProperty.Register(
            nameof(ShowRowDetailsMode), typeof(DataGridRowDetailsVisibilityMode), typeof(TitDataGrid), new PropertyMetadata(DataGridRowDetailsVisibilityMode.Collapsed));

        public DataGridRowDetailsVisibilityMode ShowRowDetailsMode
        {
            get => (DataGridRowDetailsVisibilityMode) GetValue(ShowRowDetailsModeProperty);
            set => SetValue(ShowRowDetailsModeProperty, value);
        }

        public static readonly DependencyProperty IsDateLongProperty = DependencyProperty.Register(
            nameof(IsDateLong), typeof(bool), typeof(TitDataGrid), new PropertyMetadata(default(bool)));

        public bool IsDateLong
        {
            get => (bool) GetValue(IsDateLongProperty);
            set => SetValue(IsDateLongProperty, value);
        }

        #endregion


        public TitDataGrid()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;
        }

        private static void OnAccountViewModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            IVirtualizedRefresh oldAccount = (IVirtualizedRefresh) args.OldValue;
            if (oldAccount != null)
            {
                oldAccount.PreVirtualizedRefresh -= ((TitDataGrid)sender).PreVirtualizedRefresh;
                oldAccount.PostVirtualizedRefresh -= ((TitDataGrid)sender).PostVirtualizedRefresh;
            }
            IVirtualizedRefresh newAccount = (IVirtualizedRefresh) args.NewValue;
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

        private void AddCategoryButton_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement button)
            {
                if (button.FindName("Popup") is Popup popup)
                    popup.IsOpen = true;
            }
        }

        private void AddPayeeButton_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement button)
            {
                if (button.FindName("Popup") is Popup popup)
                    popup.IsOpen = true;
            }
        }
    }
}
