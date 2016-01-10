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
    public partial class FilledMainWindow : UserControl, IRefreshCurrencyVisuals
    {
        public FilledMainWindow()
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
