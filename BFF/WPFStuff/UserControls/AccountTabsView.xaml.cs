using System.Windows.Controls;
using BFF.Helper;

namespace BFF.WPFStuff.UserControls
{
    /// <summary>
    /// Interaction logic for FilledMainWindow.xaml
    /// </summary>
    public partial class AccountTabsView : IRefreshCurrencyVisuals, IRefreshDateVisuals
    {
        public AccountTabsView()
        {
            InitializeComponent();
        }

        public void RefreshCurrencyVisuals()
        {
            NewAccountStartingBalance.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            foreach (object t in AccountsTabControl.Items)
            {
                IRefreshCurrencyVisuals grid = ((TabItem)t).Content as IRefreshCurrencyVisuals;
                grid?.RefreshCurrencyVisuals();
            }
        }

        public void RefreshDateVisuals()
        {
            foreach (object t in AccountsTabControl.Items)
            {
                IRefreshDateVisuals grid = ((TabItem)t).Content as IRefreshDateVisuals;
                grid?.RefreshDateVisuals();
            }
        }
    }
}
