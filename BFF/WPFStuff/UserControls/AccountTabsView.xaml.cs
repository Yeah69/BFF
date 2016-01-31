using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for FilledMainWindow.xaml
    /// </summary>
    public partial class AccountTabsView : UserControl, IRefreshCurrencyVisuals
    {
        public AccountTabsView()
        {
            InitializeComponent();
        }
        
        public void RefreshCurrencyVisuals()
        {
            NewAccount_StartingBalance.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            foreach (object t in AccountsTabControl.Items)
            {
                IRefreshCurrencyVisuals grid = ((TabItem) t).Content as IRefreshCurrencyVisuals;
                grid?.RefreshCurrencyVisuals();
            }
        }
    }
}
