using System.Windows.Controls;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for TitDataGrid.xaml
    /// </summary>
    public partial class TitDataGrid
    {

        public TitDataGrid()
        {
            InitializeComponent();
        }

        public void refreshCurrencyVisuals()
        {
            TitGrid.Items.Refresh();
            TotalBalanceTextBlock.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        }
    }
}
