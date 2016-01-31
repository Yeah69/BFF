using System.Windows.Controls;
using BFF.Helper;
using BFF.ViewModel;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid : IRefreshCurrencyVisuals
    {
        public TitDataGrid()
        {
            InitializeComponent();
        }

        public void RefreshCurrencyVisuals()
        {
            TitGrid.Items.Refresh();
            TotalBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }
    }
}
