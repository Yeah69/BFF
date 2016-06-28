using System.Collections;
using System.Windows;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.Model.Native;
using BFF.Model.Native.Structure;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid
    {
        public static readonly DependencyProperty AccountProperty = DependencyProperty.Register(
            nameof(Account), typeof(Account), typeof(TitDataGrid), new PropertyMetadata(default(Account)));

        public Account Account
        {
            get { return (Account) GetValue(AccountProperty); }
            set { SetValue(AccountProperty, value); }
        }

        public static readonly DependencyProperty TitsProperty = DependencyProperty.Register(
            nameof(Tits), typeof(VirtualizingObservableCollection<TitBase>), typeof(TitDataGrid), new PropertyMetadata(default(VirtualizingObservableCollection<TitBase>)));

        public VirtualizingObservableCollection<TitBase> Tits
        {
            get { return (VirtualizingObservableCollection<TitBase>) GetValue(TitsProperty); }
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
            nameof(NewTransactionCommand), typeof(ICommand), typeof(TitDataGrid), new PropertyMetadata(default(ICommand)));

        public ICommand NewTransactionCommand
        {
            get { return (ICommand) GetValue(NewTransactionCommandProperty); }
            set { SetValue(NewTransactionCommandProperty, value); }
        }

        public TitDataGrid()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;
        }
    }
}
