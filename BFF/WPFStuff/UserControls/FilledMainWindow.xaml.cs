using System.Windows.Controls;
using BFF.DB;
using BFF.Helper;
using BFF.ViewModel;
using MahApps.Metro.Controls;
using Ninject;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for FilledMainWindow.xaml
    /// </summary>
    public partial class FilledMainWindow : UserControl
    {
        public FilledMainWindow()
        {
            InitializeComponent();
        }
        
        public void refreshCurrencyVisuals()
        {
            NewAccount_StartingBalance.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            for (int i = 0; i < AccountsTabControl.Items.Count - 1; i++)
            {
                TitDataGrid titDataGrid = (TitDataGrid)((MetroTabItem)AccountsTabControl.Items[i]).Content;
                titDataGrid.RefreshCurrencyVisuals();
            }
        }
    }
}
